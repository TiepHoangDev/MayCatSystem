using System;
using System.Collections.Generic;
using System.Text;

namespace OpcHelper
{

    public interface IBaseResult<T>
    {
        public DateTime? Time { get; set; }
        public bool IsSuccess { get; set; }
        public T Value { get; set; }
        public string Message { get; set; }
    }

    public class BaseResult<T> : IBaseResult<T>
    {
        public DateTime? Time { get; set; } = DateTime.Now;
        public bool IsSuccess { get; set; } = false;
        public T Value { get; set; } = default;
        public string Message { get; set; } = null;

        public override string ToString()
        {
            if (IsSuccess)
            {
                return $"IsSuccess={IsSuccess} \t[{Time}]: \t{Value}";
            }
            return $"IsSuccess={IsSuccess} \t[{Time}]: \t{Message}";
        }


        public static implicit operator bool(BaseResult<T> result)
        {
            return result?.IsSuccess == true;
        }

        public static BaseResult<T> From(T value, bool isSuccess = true, string message = null)
        {
            return new BaseResult<T>
            {
                Value = value,
                IsSuccess = isSuccess,
                Message = message
            };
        }

        public static BaseResult<T> From(Func<T> getData)
        {
            try
            {
                var data = getData.Invoke();
                return From(data);
            }
            catch (Exception ex)
            {
                return From(default, false, ex.Message);
            }
        }
    }
}
