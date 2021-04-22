namespace vulnerable_asp_net_core.Utils
{
    public class VulnerableComponent
    {
        public static string process(string userInput)
        {
            return "<b>" + userInput + "</b>";
        }
    }
}