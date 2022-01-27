namespace OmgBacon.ModelsBuilder.Settings {
   
    public class EditorConfigSettings {

        public int IndentSize { get; set; } = 4;

        public EditorConfigIndentStyle IndentStyle { get; set; } = EditorConfigIndentStyle.Space;

        public bool SortSystemDirectoriesFirst { get; set; } = true;

    }

}