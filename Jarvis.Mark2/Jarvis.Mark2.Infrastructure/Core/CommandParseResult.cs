namespace Jarvis.Mark2.Infrastructure.Core
{
    public class CommandParseResult
    {
        public CommandType CommandType { get; set; }
        
        public SystemCommandType SystemCommandType { get; set; }
        
        //public string NormalizedText { get; set; } = string.Empty;
    }
}
