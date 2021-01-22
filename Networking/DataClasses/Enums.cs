using System;
using System.Collections.Generic;
using System.Text;

namespace Networking
{
    public enum SESSIONSTATUS
    {
        NONE,
        LOGGINGIN,
        ACCEPTINGTERMS,
        SYNCHRONIZING,
        MAINMENU,
        CHARSELECT,
        CHARCREATE,
        CHARDELETE,
        INGAME,
        DISCONNECTING,
        DISCONNECTED
    }

    public enum LOGINRESULT
    {
        SUCCESS = 0x01,
        SUCCESS_CREATE = 0x03,
        ERROR = 0x02,
        ERROR_CREATE = 0x04,
        ATTEMPT = 0x10,
        CREATE = 0x20
    }

    [Flags]
    public enum PRIVILEGE
    {
        USER = 0X01,
        ADMIN = 0X02,
        ROOT = 0X04
    }
}
