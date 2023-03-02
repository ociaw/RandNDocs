using Ociaw.StatiqMermaid;
using Ociaw.VersionedDocs;
using Statiq.Highlight;
using Statiq.Minification;

await Bootstrapper.Factory
    .CreateVersionedDocs(args)
    .AddPipeline("Mermaid", settings =>
    {
        var executable = settings.GetString("MermaidExecutable");
        return executable is null ? new Mermaid() : new Mermaid(executable);
    })
    .ModifyPipeline("Assets", pl =>
    {
        pl.PostProcessModules.Append(
            new ExecuteIf(
                Config.FromDocument(doc => doc.MediaTypeEquals(MediaTypes.JavaScript)),
                new IModule[] { new CacheDocuments(new MinifyJs()) }
            ),
            new ExecuteIf(
                Config.FromDocument(doc => doc.MediaTypeEquals(MediaTypes.Css)),
                new IModule[] { new CacheDocuments(new MinifyCss()) }
            ),
            new ExecuteIf(
                Config.FromDocument(doc => doc.MediaTypeEquals(MediaTypes.Xml) || doc.MediaTypeEquals("image/svg+xml")),
                new IModule[] { new CacheDocuments(new MinifyXml()) }
            )
        );
    })
    .ModifyPipeline("Content", pl =>
    {
        pl.PostProcessModules.Append(
            new CacheDocuments(
                new ExecuteIf(
                    Config.FromDocument(doc => doc.MediaTypeEquals(MediaTypes.Html)),
                    new HighlightCode(),
                    new RenderMermaidSvgFromHtml().WithMermaidExecutable("mmdc"),
                    new MinifyHtml()
                )
            )
        );
    })
    .RunAsync();
