using System.Collections.Generic;

namespace Trisome.WPF.Services
{
    /// <summary>
    /// An <see cref="IDialogResult"/> that contains <see cref="IDialogParameters"/> from the dialog
    /// and the <see cref="ButtonResult"/> of the dialog.
    /// </summary>
    public class DialogResult : IDialogResult
    {
        /// <summary>
        /// The result of the dialog.
        /// </summary>
        public ButtonResult Result { get; private set; }
        /// <summary>
        /// The parameters from the dialog.
        /// </summary>
        public IDictionary<string, object> Args { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogResult"/> class.
        /// </summary>
        public DialogResult()
        {
            Result = ButtonResult.None;
            Args = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogResult"/> class.
        /// </summary>
        /// <param name="result">The result of the dialog.</param>
        public DialogResult(ButtonResult result)
        {
            Result = result;
            Args = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogResult"/> class.
        /// </summary>
        /// <param name="result">The result of the dialog.</param>
        /// <param name="args">The parameters from the dialog.</param>
        public DialogResult(ButtonResult result, IDictionary<string, object> args)
        {
            Result = result;
            Args = args;
        }
    }
}
