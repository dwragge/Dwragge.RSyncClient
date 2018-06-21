using System.ComponentModel;

namespace Dwragge.RCloneClient.Common
{
    public enum RCloneSubCommand
    {
        [Description("copy")]
        Copy,

        [Description("sync")]
        Sync,

        [Description("listremotes")]
        ListRemotes
    }
}