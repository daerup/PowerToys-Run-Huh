using System.Diagnostics;
using System.IO;

namespace PowerToys_Run_Huh;

public static class NotepadHelper
{

    public static void ShowMessage(string title = null, string message = null)
    {
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = "notepad.exe";
        psi.Arguments = "";

        // Start Notepad without input/output redirection
        psi.RedirectStandardInput = false;
        psi.RedirectStandardOutput = false;
        psi.UseShellExecute = true;

        // Write the content to a temporary file
        string tempFilePath = Path.Combine(System.IO.Path.GetTempPath(), title);
        File.WriteAllText(tempFilePath, message);

        // Open the temporary file in Notepad
        Process.Start("notepad.exe", tempFilePath);
    }
}