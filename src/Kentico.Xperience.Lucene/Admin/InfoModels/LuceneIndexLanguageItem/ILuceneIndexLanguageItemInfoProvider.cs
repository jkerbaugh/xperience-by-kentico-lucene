using CMS.DataEngine;

namespace Kentico.Xperience.Lucene.Admin;

/// <summary>
/// Declares members for <see cref="LuceneIndexLanguageItemInfo"/> management.
/// </summary>
public partial interface ILuceneIndexLanguageItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}
