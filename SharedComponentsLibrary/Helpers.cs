using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LanguageLibrary;

namespace SharedComponentsLibrary
{
    public static class Helpers
    {
        public static async Task<ContentDialogResult> DisplayMessageDialog(string message, string header)
        {
            
            return await DisplayQuestionDialog(message, Resources.Ok, "", header);
        }

        public static async Task<ContentDialogResult> DisplayQuestionDialog(string question, string primaryButtonText="Ok", string secondaryButtonText="Cancel",string header="")
        {
            if (primaryButtonText == "Ok")
                primaryButtonText = Resources.Ok;
            if(secondaryButtonText == "Cancel")
                secondaryButtonText = Resources.Cancel;
            MyContentDialog dialog = new MyContentDialog()
            {
                Content = question,
                PrimaryButtonText = primaryButtonText,
                SecondaryButtonText = secondaryButtonText,
                Title = header
            };
            await ContentDialogMaker.CreateContentDialogAsync(dialog, true);
            return ContentDialogMaker.Result;
        }
    }
}
