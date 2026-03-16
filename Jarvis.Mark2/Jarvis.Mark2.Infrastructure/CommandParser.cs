namespace Jarvis.Mark2.Infrastructure
{
    public class CommandParser
    {
        private readonly HashSet<string> listWakeUp = ["привет", "джарвис","жарвис","дарвис", "джервис", "джарвис не спишь","привет джарвис"];
        private readonly HashSet<string> listSleep = ["пока", "джарвис спать","спать","спящий режим"];
        private readonly string muteCommand = "без звука";
        private readonly string unMuteCommand = "верни звук";
        private readonly HashSet<string> listGoogle = ["открой гугл", "гугл"];
        private readonly HashSet<string> listYouTube = ["открой ютуб", "ютуб"];

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

            //AllCommand
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

        //AllCommand
        private SystemCommandType GetSystemCommand(string text)
        {
            if (muteCommand.Equals(text))
                return SystemCommandType.Mute;

            if (unMuteCommand.Equals(text))
                return SystemCommandType.UnMute;

            if (listGoogle.Contains(text) || listGoogle.Any(text.Contains))
                return SystemCommandType.OpenGoogle;

            if (listYouTube.Contains(text) || listYouTube.Any(text.Contains))
                return SystemCommandType.OpenYouTube;

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
