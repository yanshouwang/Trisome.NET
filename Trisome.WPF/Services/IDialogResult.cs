using System.Collections.Generic;

namespace Trisome.WPF.Services
{
    /// <summary>
    /// Contains <see cref="IDialogParameters"/> from the dialog
    /// and the <see cref="ButtonResult"/> of the dialog.
    /// </summary>
    public interface IDialogResult
    {
        /// <summary>
        /// The parameters from the dialog.
        /// </summary>
        IDictionary<string, object> Args { get; }
        /// <summary>
        /// The result of the dialog.
        /// </summary>
        ButtonResult Result { get; }
    }
}