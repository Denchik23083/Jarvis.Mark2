
namespace Jarvis.Mark2.Infrastructure.Core
{
    public class CommandParser
    {
        private readonly HashSet<string> listWakeUp = ["при", "привет", "джейв", "джарвис","жарвис","дарвис", "джервис", "джарвис не спишь","привет джарвис"];
        private readonly HashSet<string> listSleep = ["пока", "джарвис спать","спать","спящий режим"];
        private readonly string[] jarvisAliases = ["джейв", "джарвис", "жарвис", "джервис", "дарвис"];

        private readonly Dictionary<SystemCommandType, string[]> systemCommands = new()
        {
            { SystemCommandType.Mute, ["без звука"] },
            { SystemCommandType.UnMute, ["верни звук"] },
            { SystemCommandType.Clear, ["удали чат", "удалить чат", "очисти чат"] },
            { SystemCommandType.OpenGoogle, ["открой гугл", "гугл"] },
            { SystemCommandType.OpenYouTube, ["открой ютуб", "ютуб"] },
            { SystemCommandType.OpenSteam, ["открой стим", "стим"] },
            { SystemCommandType.OpenWot, ["открой танки", "танки"] }
        };

        public CommandParseResult Parse(string text, bool isActivated)
        {
            text = Normalize(text);

            if (string.IsNullOrWhiteSpace(text))
            {
                return new CommandParseResult
                {
                    CommandType = CommandType.None,
                    SystemCommandType = SystemCommandType.None
                };
            }

            if (!isActivated)
            {
                if (IsWakeCommand(text))
                {
                    return new CommandParseResult
                    {
                        CommandType = CommandType.Wake,
                        SystemCommandType = SystemCommandType.None
                    };
                }

                return new CommandParseResult
                {
                    CommandType = CommandType.None,
                    SystemCommandType = SystemCommandType.None
                };
            }

            if (IsSleepCommand(text))
            {
                return new CommandParseResult
                {
                    CommandType = CommandType.Sleep,
                    SystemCommandType = SystemCommandType.None
                };
            }

            var systemCommand = GetSystemCommand(text);
            if (systemCommand != SystemCommandType.None)
            {
                return new CommandParseResult
                {
                    CommandType = CommandType.System,
                    SystemCommandType = systemCommand
                };
            }

            return new CommandParseResult
            {
                CommandType = CommandType.AiQuery,
                SystemCommandType = SystemCommandType.None
            };
        }

        public bool IsWakeCommand(string text)
        {
            if (listWakeUp.Contains(text))
                return true;

            return listWakeUp.Any(text.Contains);
        }

        public bool IsSleepCommand(string text)
        {
            if (listSleep.Contains(text))
                return true;

            return listSleep.Any(text.Contains);
        }

        public string CleanAiText(string text)
        {
            text = Normalize(text);

            foreach (var alias in jarvisAliases)
            {
                if (text.StartsWith(alias + " "))
                {
                    text = text[(alias.Length + 1)..];
                    break;
                }

                if (text == alias)
                {
                    return string.Empty;
                }
            }

            return text.Trim();
        }

        private SystemCommandType GetSystemCommand(string text)
        {
            foreach (var command in systemCommands)
            {
                if (command.Value.Any(trigger => 
                    text.Equals(trigger, StringComparison.OrdinalIgnoreCase) ||
                    text.Contains(trigger, StringComparison.OrdinalIgnoreCase)))
                {
                    return command.Key;
                }
            }

            return SystemCommandType.None;
        }

        private static string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return text
                .Trim()
                .ToLower()
                .Replace("  ", " ");
        }
    }
}
