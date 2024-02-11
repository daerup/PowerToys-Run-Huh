using Wox.Plugin;
using System.Windows.Controls;
using Microsoft.PowerToys.Settings.UI.Library;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Windows;
using PowerToys_Run_Huh.types;
using System.Windows.Controls.Primitives;
using Wox.Plugin.Common.Win32;

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
    private const int MaxScore = Int32.MaxValue-1000;
    private const string EndChar = "§";
    public static string PluginID => "KC0B1WNT80BXS3FKWOQPYRXRU8QHJKB6";
    public string Name => "Huh";
    public string Description => "HUH?!?!?.";
    internal string? Endpoint { get; private set; }
    internal string? Model { get; private set; }
    internal string? Instruction { get; private set; }
    private string ImageDirectory  => "images/";
    private Action<string> Requery;
    private Action<string> ShowText;

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
        Requery = query => context.API.ChangeQuery(query, true);
        ShowText = text => context.API.ChangeQuery(text, false);
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
        var results = new List<Result>();
        var message = query.RawQuery.EndsWith(EndChar) ? query.RawQuery[..^1] : query.RawQuery;
        results.Add(new Result
        {
            Title = message,
            SubTitle = "Search using AI ",
            IcoPath = Path.Combine(this.ImageDirectory, "ai.png"),
            Score = MaxScore - 100,
            ContextData = new ContextData(ResponseType.Question),
            Action = _ =>
            {
                this.Requery(query.Search + EndChar);
                this.ShowText("Searching...");
                return false;
            }
        });

        if (!query.RawQuery.EndsWith(EndChar))
        {
            return results;
        }

        var requestBody = new
        {
            model = this.Model,
            temperature = 0.5,
            presence_penalty = 0,
            stream = false,
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

        var result = new Result();
        try
        {
            HttpResponseMessage response = client.PostAsync("http://localhost:8080/v1/chat/completions", content)
                                                 .GetAwaiter().GetResult();
            var answer = response
                         .Content
                         .ReadFromJsonAsync<ChatCompletionResponse>()
                         .GetAwaiter()
                         .GetResult()!
                         .Choices.Last().Message.Content;

            int breakInterval = 100;
            string formattedAnswer = answer.Length > breakInterval
                ? string.Join(Environment.NewLine,
                              Enumerable.Range(0, answer.Length / breakInterval)
                                        .Select(i => answer.Substring(i * breakInterval, breakInterval)))
                : answer;
            result.Title = message;
            result.SubTitle = formattedAnswer;
            result.FontFamily = "Consolas";
            result.IcoPath = Path.Combine(this.ImageDirectory, "speak.png");
            result.Score = MaxScore;
            result.ContextData = new ContextData(ResponseType.Answer, answer);
            result.Action = _ =>
            {
                Clipboard.SetText(answer);
                return true;
            };
        }
        catch (Exception e)
        {
            result.Title = "Error :(";
            result.SubTitle = e.Message;
            result.IcoPath = Path.Combine(this.ImageDirectory, "stop.png");
            result.Score = MaxScore;
            result.ContextData = new ContextData(ResponseType.Error);
        }
        finally
        {
            results.Add(result);
        }

        return results;
    }

    public List<ContextMenuResult> LoadContextMenus(Result result)
    {
        var results = new List<ContextMenuResult>();
        
        var contextData = result.ContextData as ContextData;

        if (contextData?.ResponseType != ResponseType.Answer )
        {
            return results;
        }
        string message = contextData.Message ?? result.SubTitle;
        results.Add(new ContextMenuResult
        {
            Title = "Copy to clipboard",
            Glyph = "\xE8C8",
            FontFamily = "Segoe MDL2 Assets",
            Action = _ =>
            {
                Clipboard.SetText(message);
                return true;
            }
        }); 
        results.Add(new ContextMenuResult
        {
            Title = "Open in Notepad",
            Glyph = "\xE8E5",
            FontFamily = "Segoe MDL2 Assets",
            Action = _ =>
            {
                Task.Run(() => NotepadHelper.Open(message));
                return true;
            }
        });
        return results;
    }
}