using System.Reflection;

namespace RandomTrainTrailers.Detour
{
    public class DetourItem
    {
        string name;
        MethodInfo original;
        MethodInfo detour;
        RedirectCallsState state;
        bool deployed;

        public bool Deployed { get { return deployed; } }

        public DetourItem(string name, MethodInfo original, MethodInfo detour)
        {
            this.name = name;
            this.original = original;
            this.detour = detour;
        }

        public void Deploy()
        {
            if(deployed || original == null || detour == null)
            {
                var str = "Detour not possible for " + name;
                str += deployed ? "\r\nAlready deployed" : "";
                str += original == null? "\r\nNo original" : "";
                str += detour == null ? "\r\nNo detour" : "";
                Util.LogWarning(str);
                return;
            }

            Util.Log(name + " redirected!", true);
            deployed = true;
            state = RedirectionHelper.RedirectCalls(original, detour);
        }

        public void Revert()
        {
            if(!deployed || original == null)
                return;

            Util.Log(name + " restored!", true);
            deployed = false;
            RedirectionHelper.RevertRedirect(original, state);
        }
    }
}
