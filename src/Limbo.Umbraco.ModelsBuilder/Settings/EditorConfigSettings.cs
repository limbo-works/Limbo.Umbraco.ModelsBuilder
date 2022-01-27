namespace Limbo.Umbraco.ModelsBuilder.Settings {

    /// <summary>
    /// Class representing (some of) the settings in an <c>.editorconfig</c> file.
    /// </summary>
    public class EditorConfigSettings {

        /// <summary>
        /// Gets the indent size.
        /// </summary>
        public int IndentSize { get; set; } = 4;

        /// <summary>
        /// Gets the indent style.
        /// </summary>
        public EditorConfigIndentStyle IndentStyle { get; set; } = EditorConfigIndentStyle.Space;

        /// <summary>
        /// Gets whether <c>System</c> imports should be sorted first.
        /// </summary>
        public bool SortSystemDirectoriesFirst { get; set; } = true;

    }

}