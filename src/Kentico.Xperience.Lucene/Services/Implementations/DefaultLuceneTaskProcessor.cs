using CMS.Base;
using CMS.Core;
using CMS.Websites;
using Kentico.Xperience.Lucene.Models;
using Lucene.Net.Documents;
using Lucene.Net.Documents.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Kentico.Xperience.Lucene.Constants;

namespace Kentico.Xperience.Lucene.Services;

internal class LuceneBatchResult
{
    internal int SuccessfulOperations { get; set; } = 0;
    internal HashSet<LuceneIndex> PublishedIndices { get; set; } = new();
}


internal class DefaultLuceneTaskProcessor : ILuceneTaskProcessor
{
    private readonly IWebPageUrlRetriever urlRetriever;
    private readonly IServiceProvider serviceProvider;
    private readonly ILuceneClient luceneClient;
    private readonly IEventLogService eventLogService;


    public DefaultLuceneTaskProcessor(
        ILuceneClient luceneClient,
        IEventLogService eventLogService,
        IWebPageUrlRetriever urlRetriever,
        IServiceProvider serviceProvider)
    {
        this.luceneClient = luceneClient;
        this.eventLogService = eventLogService;
        this.urlRetriever = urlRetriever;
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task<int> ProcessLuceneTasks(IEnumerable<LuceneQueueItem> queueItems, CancellationToken cancellationToken, int maximumBatchSize = 100)
    {
        LuceneBatchResult batchResults = new();

        var batches = queueItems.Batch(maximumBatchSize);

        foreach (var batch in batches)
        {
            await ProcessLuceneBatch(batch, cancellationToken, batchResults);
        }

        foreach (var index in batchResults.PublishedIndices)
        {
            var storage = index.StorageContext.GetNextOrOpenNextGeneration();
            index.StorageContext.PublishIndex(storage);
        }

        return batchResults.SuccessfulOperations;
    }

    private async Task ProcessLuceneBatch(IEnumerable<LuceneQueueItem> queueItems, CancellationToken cancellationToken, LuceneBatchResult previousBatchResults)
    {

        var groups = queueItems.GroupBy(item => item.IndexName);

        foreach (var group in groups)
        {
            try
            {
                var deleteIds = new List<string>();
                var deleteTasks = group.Where(queueItem => queueItem.TaskType == LuceneTaskType.DELETE).ToList();

                var updateTasks = group.Where(queueItem => queueItem.TaskType is LuceneTaskType.PUBLISH_INDEX or LuceneTaskType.UPDATE);
                var upsertData = new List<Document>();
                foreach (var queueItem in updateTasks)
                {
                    var document = await GetDocument(queueItem);
                    if (document is not null)
                    {
                        upsertData.Add(document);
                    }
                    else
                    {
                        deleteTasks.Add(queueItem);
                    }
                }
                deleteIds.AddRange(GetIdsToDelete(deleteTasks ?? new List<LuceneQueueItem>()).Where(x => x is not null).Select(x => x ?? ""));
                if (IndexStore.Instance.GetIndex(group.Key) is { } index)
                {
                    previousBatchResults.SuccessfulOperations += await luceneClient.DeleteRecords(deleteIds, group.Key);
                    previousBatchResults.SuccessfulOperations += await luceneClient.UpsertRecords(upsertData, group.Key, cancellationToken);

                    if (group.Any(t => t.TaskType == LuceneTaskType.PUBLISH_INDEX) && !previousBatchResults.PublishedIndices.Any(x => x.IndexName == index.IndexName))
                    {
                        previousBatchResults.PublishedIndices.Add(index);
                    }
                }
                else
                {
                    eventLogService.LogError(nameof(DefaultLuceneTaskProcessor), nameof(ProcessLuceneTasks), "Index instance not exists");
                }
            }
            catch (Exception ex)
            {
                eventLogService.LogError(nameof(DefaultLuceneTaskProcessor), nameof(ProcessLuceneTasks), ex.Message);
            }
        }
    }

    private static IEnumerable<string?> GetIdsToDelete(IEnumerable<LuceneQueueItem> deleteTasks) => deleteTasks.Select(queueItem => queueItem.IndexedItemModel.WebPageItemGuid.ToString());

    /// <inheritdoc/>
    public async Task<Document?> GetDocument(LuceneQueueItem queueItem)
    {
        var luceneIndex = IndexStore.Instance.GetIndex(queueItem.IndexName) ?? throw new Exception($"LuceneIndex {queueItem.IndexName} not found!");

        var strategy = serviceProvider.GetRequiredStrategy(luceneIndex);

        var data = await strategy!.MapToLuceneDocumentOrNull(queueItem.IndexedItemModel);

        if (data is null)
        {
            return null;
        }

        await AddBaseProperties(queueItem.IndexedItemModel, data!);

        return data;
    }

    private async Task AddBaseProperties(IndexedItemModel lucenePageItem, Document document)
    {
        document.AddStringField(BaseProperties.CLASS_NAME, lucenePageItem.ContentTypeName, Field.Store.YES);
        document.AddStringField(BaseProperties.WEB_PAGE_ITEM_GUID, lucenePageItem.WebPageItemGuid.ToString(), Field.Store.YES);
        document.AddStringField(BaseProperties.LANGUAGE_NAME, lucenePageItem.LanguageName, Field.Store.YES);

        if (!document.Any(x => x.Name == BaseProperties.URL))
        {
            string url = string.Empty;
            try
            {
                url = (await urlRetriever.Retrieve(lucenePageItem.WebPageItemGuid, lucenePageItem.LanguageName)).RelativePath;
            }
            catch (Exception)
            {
                // Retrieve can throw an exception when processing a page update LuceneQueueItem
                // and the page was deleted before the update task has processed. In this case, upsert an
                // empty URL
            }

            document.AddStringField(BaseProperties.URL, url, Field.Store.YES);
        }
    }
}
