using System;

namespace BehaviourTree.ExTools
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OpenViewAttribute : Attribute
    {
        public string ButtonName = "打开视图";
    }
}