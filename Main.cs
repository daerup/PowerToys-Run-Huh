using Wox.Plugin;
using System.Windows.Controls;
using Microsoft.PowerToys.Settings.UI.Library;
using System.IO;
using ManagedCommon;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using PowerToys_Run_Huh.types;
using static System.Net.Mime.MediaTypeNames;

// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable once UnusedMember.Global
// ReSharper disable once InconsistentNaming
// ReSharper disable once UnusedAutoPropertyAccessor.Global
#pragma warning disable IDE0008
#pragma warning disable IDE0063
#pragma warning disable CA1416

namespace PowerToys_Run_Huh;
public class Main : IPlugin, IContextMenu, ISettingProvider
{
    public static string PluginID => "KC0B1WNT80BXS3FKWOQPYRXRU8QHJKB6";
    public string Name => "Huh";
    public string Description => "HUH?!?!?.";
    internal string? Endpoint { get; private set; }
    internal string? Model { get; private set; }
    internal string? Instruction { get; private set; }
    private string ImageDirectory  => "images/";


    IEnumerable <PluginAdditionalOption> ISettingProvider.AdditionalOptions => new List<PluginAdditionalOption>()
    {
        new()
        {
            Key = nameof(this.Endpoint),
            DisplayLabel = "Endpoint",
            DisplayDescription = "The Endpoint that queries are sent to. Can be also be OpenAI API (at least I think?)",
            PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
            TextValue = "http://localhost:8080/v1/chat/completions",
        },
        new()
        {
            Key = nameof(this.Model),
            DisplayLabel = "Model",
            DisplayDescription = "The model that queries are sent to.",
            PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
            TextValue = "gpt-4-1106-preview",
        },
        new()
        {
            Key = nameof(this.Instruction),
            DisplayLabel = "Instruction",
            DisplayDescription = "The Instruction that is sent with the query",
            PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
            TextValue = "Answer all question as fast and correct as possible. Don't write full sentences and only provide the asked for information. No need to pad your response with anything. Just answer"    
        }
    };

    public void Init(PluginInitContext context)
    {
    }

    public Control CreateSettingPanel() => throw new NotImplementedException();

    public void UpdateSettings(PowerLauncherPluginSettings settings)
    {
        this.Endpoint = (string)GetSettingOrDefault(settings, nameof(this.Endpoint));
    }

    private object GetSettingOrDefault(
        PowerLauncherPluginSettings settings,
        string key
    )
    {
        var defaultOptions = ((ISettingProvider)this).AdditionalOptions;
        var defaultOption = defaultOptions.First(x => x.Key == key);
        var option = settings?.AdditionalOptions?.FirstOrDefault(x => x.Key == key);

        return defaultOption.PluginOptionType switch
        {
            PluginAdditionalOption.AdditionalOptionType.Textbox => option?.TextValue ?? defaultOption.TextValue,
            _ => throw new NotSupportedException()
        };
    }

    public List<Result> Query(Query query)
    {
        if (!query.RawQuery.EndsWith('.'))
        {
            return
            [
                new Result
                {
                    Title = "Waiting for message completion...",
                    SubTitle = "To complete the message, end it with a period (.)",
                    IcoPath = Path.Combine(this.ImageDirectory, "question.png")
        }
            ];
        }
        
        var message = query.RawQuery[..^1];

        var requestBody = new
        {
            model = this.Model,
            messages = new[]
            {
                new { role = "system", content = this.Instruction },
                new { role = "user", content = message }!
            }
        };

        string jsonRequestBody = JsonSerializer.Serialize(requestBody);

        using var client = new HttpClient();
        var content = new StringContent(
            jsonRequestBody,
            Encoding.UTF8,
            "application/json"
        );

        HttpResponseMessage response = client.PostAsync("http://localhost:8080/v1/chat/completions", content).GetAwaiter().GetResult();
        if (!response.IsSuccessStatusCode)
        {
            return
            [
                new Result
                {
                    Title = "Error :(",
                    SubTitle = $"LMAO Get fucked, failed with status code: {response.StatusCode} (Did you forget to run the container you headass)",
                    IcoPath = Path.Combine(this.ImageDirectory, "question.png")
                }
            ];
        }

        var result = new Result();
        try
        {
            var answer = response
                         .Content
                         .ReadFromJsonAsync<ChatCompletionResponse>()
                         .GetAwaiter()
                         .GetResult()!
                         .Choices.Last().Message.Content;

            result.SubTitle = answer;
            result.FontFamily = "Consolas";
            result.IcoPath = Path.Combine(this.ImageDirectory, "ai.png");
            result.Action = _ =>
            {
                Task.Run(() => NotepadHelper.ShowMessage("ChadGPT response",answer));
                return true;
            };
        }
        catch (Exception e)
        {
            result.Title = "Error :(";
            result.SubTitle = "Could not parse response correctly.";
            result.IcoPath = Path.Combine(this.ImageDirectory, "stop.png");
        }
        return [result ];
    }

    public List<ContextMenuResult> LoadContextMenus(Result result)
    {
        var results = new List<ContextMenuResult>();
        return results;
    }
}