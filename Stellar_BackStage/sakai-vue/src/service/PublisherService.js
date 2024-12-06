import { getAllPublishers, editPublisherApi } from "@/api/index";

export const PublisherService = {
    //READ
    async getPublishers() {
        try {
          const data = await getAllPublishers(); // 等待資料返回
          return data.body;                      // 返回結果的 body
        } catch (err) {
          throw err;                             // 拋出錯誤
        }
      },
      //變更
      async updatePublisher(publisher) {
        
        
        try {
          const data = await editPublisherApi(publisher); // 等待 API 回應
          return data.body;                      // 返回結果的 body
        } catch (err) {
          throw err;                             // 拋出錯誤
        }
      }
    
};