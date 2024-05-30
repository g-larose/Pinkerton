using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinkerton.Models
{
    public class SystemErrors
    {
        public static Dictionary<string, Guid> Errors { get; set; } = new Dictionary<string, Guid>()
        {
           { "Guided", Guid.Parse("110d6167-b3d7-4376-bead-adaa305eff3b") },
           { "Member Not Found", Guid.Parse("79b68019-a3b2-4dec-a1ae-5dce7fc9f5d8") },
           { "Message does not exist", Guid.Parse("b44d854e-6d57-4a4d-b8de-0fd8733d2f66") },
           { "Member Exist", Guid.Parse("5a79d6e3-ba3b-426f-8f75-e4217fdcbd4a") },
           { "Index out of range", Guid.Parse("2520717d-b227-4450-8710-7bd3c6f0c8da") },
           { "Timeout", Guid.Parse("d4901ab0-328b-4c39-874e-cfb32120144d") },
           { "ErrorCode Not Found", Guid.Parse("4b535f4f-2d61-4e02-8e95-87ebe89cacf5") },
           { "Unkown Error", Guid.Parse("981aae7e-babb-4646-bcff-2e85fac48894") },
           { "CanCreateChats", Guid.Parse("44b57d5e-1600-4f44-8cc8-158e6bd2954a") },
           { "CanSendMessages", Guid.Parse("2fba6b13-1be0-4080-aa9b-5fe3936b2a74") },
           { "NullReferenceException", Guid.Parse("c4d2538c-9c97-463b-ad8c-cdce739a51d1") },
           { "TransientError", Guid.Parse("2ebdc363-4192-43ce-963a-0e7b2a20844f") }
        };

        public static Guid GetError(string error, Guid defaultValue)
        {
            if (Errors.TryGetValue(error, out Guid value)) return value;
            return defaultValue;
        }
    }
}
