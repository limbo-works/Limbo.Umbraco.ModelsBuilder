using System;
using System.Text;

namespace Limbo.Umbraco.ModelsBuilder.Logging {
    
    /// <summary>
    /// Class representing a log.
    /// </summary>
    public class ModelsBuilderLog {

        private readonly StringBuilder _sb;

        #region Properties

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ModelsBuilderLog() {
            _sb = new StringBuilder();
        }

        #endregion

        #region Member methods

        /// <summary>
        /// Appends a new, empty line to the log.
        /// </summary>
        public void AppendLine() {
            _sb.AppendLine();
        }

        /// <summary>
        /// Appends the specified <paramref name="line"/> to the log.
        /// </summary>
        /// <param name="line">The line to be added.</param>
        public void AppendLine(string line) {
            if (string.IsNullOrWhiteSpace(line)) {
                _sb.AppendLine();
            } else {
                _sb.AppendLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] {line}");
            }
        }

        /// <inheritdoc />
        public override string ToString() {
            return _sb.ToString();
        }

        #endregion

    }

}