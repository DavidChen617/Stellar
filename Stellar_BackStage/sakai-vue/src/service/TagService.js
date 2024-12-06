import { getTags,createTag,updateTag,deleteTag} from "@/api/index";

export const TagService = {
    async fetchTags(){
        try {
            const respose=await getTags() ;
            return respose;
        }catch(err){
            console.error("Error fetching tags:", error); // 日誌錯誤
            throw new Error("Failed to fetch tags"); // 提供錯誤上下文
        }
    },

    async addTags(tag){
        try {
            await createTag(tag) ;
            console.log(tag)
            
        }catch(err){
            console.error("Error add tag:", err); // 日誌錯誤
            throw new Error("Failed to add tag"); // 提供錯誤上下文
        }
    },
    async modifyTag(tag){
        try {
           await updateTag(tag) ;
          
        }catch(err){
            console.error("Error fetching tag:", error); 
            throw new Error("Failed to fetch tag"); 
        }
    },
    async removeTag(tagId){
        try {
           await deleteTag(tagId) ;
           
        }catch(err){
            console.error("Error fetching tag:", error); 
            throw new Error("Failed to fetch tag"); 
        }
    },

}