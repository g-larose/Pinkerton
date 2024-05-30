using Pinkerton.BaseModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinkerton.Services
{
    public class UtilityProviderService : IDisposable
    {
        #region DISPOSE
        public void Dispose()
        {
            DisposableBase disposableBase = new();
            disposableBase.Dispose();
        }
        #endregion

        #region PARSE DURATION
        public TimeSpan ParseDuration(string input)
        {
            string amountPart = "";
            string timePart = "";
            if (input.Length < 3)
            {
                amountPart = input.Substring(0, 1);
                timePart = input.Substring(1, 1);
            }   
            else
            {
                amountPart = input.Substring(0, 2);
                timePart = input.Substring(2, 1);
            }
            return new TimeSpan();
        }
        #endregion
    }
}
