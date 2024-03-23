using Pinkerton.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinkerton.Interfaces
{
    public interface IWelcomeProvider
    {
        Task<WelcomeMessage> GetRandomWelcomeMessageAsync();
    }
}
