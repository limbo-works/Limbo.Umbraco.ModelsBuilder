using Limbo.Umbraco.ModelsBuilder.Settings;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;

namespace Limbo.Umbraco.ModelsBuilder.Services {

    /// <summary>
    /// Class responsible for loading the dependencies for <see cref="ModelsGenerator"/>. This is done make extending
    /// the <see cref="ModelsGenerator"/> class easier as well as avoid breaking changes should we need another
    /// dependency in the future.
    /// </summary>
    public class ModelsGeneratorDependencies {

        #region Properties
        
        /// <summary>
        /// Gets a reference to the current <see cref="IEventAggregator"/>.
        /// </summary>
        public IEventAggregator EventAggregator { get; }

        /// <summary>
        /// Gets a reference to the current <see cref="IHostingEnvironment"/>.
        /// </summary>
        public IHostingEnvironment HostingEnvironment { get; }
        
        /// <summary>
        /// Gets a reference to the current <see cref="IContentTypeService"/>.
        /// </summary>
        public IContentTypeService ContentTypeService { get; }
        
        /// <summary>
        /// Gets a reference to the current <see cref="IMemberTypeService"/>.
        /// </summary>
        public IMemberTypeService MemberTypeService { get; }
        
        /// <summary>
        /// Gets a reference to the current <see cref="IMediaTypeService"/>.
        /// </summary>
        public IMediaTypeService MediaTypeService { get; }
        
        /// <summary>
        /// Gets a reference to the current <see cref="IPublishedContentTypeFactory"/>.
        /// </summary>
        public IPublishedContentTypeFactory PublishedContentTypeFactory { get; }
        
        /// <summary>
        /// Gets a reference to the current <see cref="LimboModelsBuilderSettings"/>.
        /// </summary>
        public IOptions<LimboModelsBuilderSettings> ModelsBuilderSettings { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance based on the specified dependencies.
        /// </summary>
        /// <param name="eventAggregator"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="contentTypeService"></param>
        /// <param name="memberTypeService"></param>
        /// <param name="mediaTypeService"></param>
        /// <param name="publishedContentTypeFactory"></param>
        /// <param name="modelsBuilderSettings"></param>
        public ModelsGeneratorDependencies(
            IEventAggregator eventAggregator,
            IHostingEnvironment hostingEnvironment,
            IContentTypeService contentTypeService,
            IMemberTypeService memberTypeService,
            IMediaTypeService mediaTypeService,
            IPublishedContentTypeFactory publishedContentTypeFactory,
            IOptions<LimboModelsBuilderSettings> modelsBuilderSettings) {
            EventAggregator = eventAggregator;
            HostingEnvironment = hostingEnvironment;
            ContentTypeService = contentTypeService;
            MemberTypeService = memberTypeService;
            MediaTypeService = mediaTypeService;
            PublishedContentTypeFactory = publishedContentTypeFactory;
            ModelsBuilderSettings = modelsBuilderSettings;
        }

        #endregion

    }

}