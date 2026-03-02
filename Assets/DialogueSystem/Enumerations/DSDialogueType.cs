namespace DialogueSystem.Enumerations
{
    /// <summary>
    /// 对话类型枚举
    /// 定义对话系统的交互方式，控制玩家在对话中的选择行为
    /// </summary>
    public enum DSDialogueType
    {
        /// <summary>
        /// 单选对话
        /// 玩家只能从多个选项中选择一个，对话将根据选择继续
        /// 通常用于分支对话树
        /// </summary>
        SingleChoice,
        
        /// <summary>
        /// 多选对话
        /// 玩家可以选择多个选项，对话可能根据多个选择产生不同结果
        /// 通常用于收集信息或多条件触发
        /// </summary>
        MultipleChoice,
    }
}
