using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using Kentico.Xperience.Lucene.Admin;

[assembly: RegisterObjectType(typeof(LuceneIndexItemInfo), LuceneIndexItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.Lucene.Admin
{
    /// <summary>
    /// Data container class for <see cref="LuceneIndexItemInfo"/>.
    /// </summary>
    [Serializable]
    public partial class LuceneIndexItemInfo : AbstractInfo<LuceneIndexItemInfo, ILuceneIndexItemInfoProvider>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "lucene.luceneindexitem";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new(typeof(LuceneIndexItemInfoProvider), OBJECT_TYPE, "Lucene.LuceneIndexItem", nameof(LuceneIndexItemId), null, nameof(LuceneIndexItemGuid), nameof(LuceneIndexItemIndexName), null, null, null, null)
        {
            TouchCacheDependencies = true,
            ContinuousIntegrationSettings =
            {
                Enabled = true,
            },
        };


        /// <summary>
        /// Lucene index item id.
        /// </summary>
        [DatabaseField]
        public virtual int LuceneIndexItemId
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(LuceneIndexItemId)), 0);
            set => SetValue(nameof(LuceneIndexItemId), value);
        }


        /// <summary>
        /// Lucene index item Guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid LuceneIndexItemGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(LuceneIndexItemGuid)), default);
            set => SetValue(nameof(LuceneIndexItemGuid), value);
        }


        /// <summary>
        /// Index name.
        /// </summary>
        [DatabaseField]
        public virtual string LuceneIndexItemIndexName
        {
            get => ValidationHelper.GetString(GetValue(nameof(LuceneIndexItemIndexName)), String.Empty);
            set => SetValue(nameof(LuceneIndexItemIndexName), value);
        }


        /// <summary>
        /// Channel name.
        /// </summary>
        [DatabaseField]
        public virtual string LuceneIndexItemChannelName
        {
            get => ValidationHelper.GetString(GetValue(nameof(LuceneIndexItemChannelName)), String.Empty);
            set => SetValue(nameof(LuceneIndexItemChannelName), value);
        }


        /// <summary>
        /// Strategy name.
        /// </summary>
        [DatabaseField]
        public virtual string LuceneIndexItemStrategyName
        {
            get => ValidationHelper.GetString(GetValue(nameof(LuceneIndexItemStrategyName)), String.Empty);
            set => SetValue(nameof(LuceneIndexItemStrategyName), value);
        }


        /// <summary>
        /// Rebuild hook.
        /// </summary>
        [DatabaseField]
        public virtual string LuceneIndexItemRebuildHook
        {
            get => ValidationHelper.GetString(GetValue(nameof(LuceneIndexItemRebuildHook)), String.Empty);
            set => SetValue(nameof(LuceneIndexItemRebuildHook), value, String.Empty);
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            Provider.Delete(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            Provider.Set(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected LuceneIndexItemInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="LuceneIndexItemInfo"/> class.
        /// </summary>
        public LuceneIndexItemInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="LuceneIndexItemInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public LuceneIndexItemInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}
