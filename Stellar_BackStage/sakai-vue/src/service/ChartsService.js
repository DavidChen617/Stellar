
import { getCharts } from "@/api/index";



export const getAllChartsService = {

    
      async getCharts() {
        try {
          const data = await getCharts(); // 等待資料返回
          return data;                      // 返回結果的 body
          } catch (err) {
            throw err;                             // 拋出錯誤
          }


    },
};
