using Microsoft.Extensions.Caching.Distributed;

namespace Web.Helpers
{
    static public class CacheHelper
    {

        public static async Task<T> TryGetFromCacheAsync<T>(IDistributedCache cache,string cacheKey) where T : class
        {
            var cacheValue = await cache.GetAsync(cacheKey); 
            var cachedProducViewModel = SerializationHelper.ByteArrayToObj<T>(cacheValue);
            return cachedProducViewModel;
        }
        //                                                                                              滑動過期時間(可選參數 , 預設3分鐘)         絕對過期(可選參數 , 預設10分鐘)   
        public static async Task SetCachedAsync<T>(IDistributedCache cache,string cacheKey, T model, TimeSpan? slidingExpiration = null, TimeSpan? absoluteExpirationRelativeToNow = null)
        {
            if (slidingExpiration is null) slidingExpiration = TimeSpan.FromMinutes(3);
            if (absoluteExpirationRelativeToNow is null) slidingExpiration = TimeSpan.FromMinutes(10);

            var byteResult = SerializationHelper.ObjectToByteArray(model);

            await cache.SetAsync(cacheKey, byteResult, new DistributedCacheEntryOptions()
            {
                SlidingExpiration = slidingExpiration,
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
            });
        }

    }
}
