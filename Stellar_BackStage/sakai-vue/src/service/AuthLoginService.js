import { login } from "@/api/index";

export const loginService = {
    async login(request){
        try {
            const respose=await login(request) ;
            return respose;
        }catch(err){
            console.error("error login:", error); // 日誌錯誤
            throw new Error("error login"); // 提供錯誤上下文
        }
    }
}
