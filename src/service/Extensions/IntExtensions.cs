using System;

namespace service.Extensions
{
    public static class IntExtensions
    {
        public static double GetPercentage(this int num, int total)
        {
            if(total == 0)
                return 0;

            return Math.Round((double)num / total * 100, 2);
        }
    }
}