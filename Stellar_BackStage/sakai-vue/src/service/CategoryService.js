
import { getCategories, createCategory, updateCategory, deleteCategory } from "@/api/index"; // 確保 api/index.js 中有正確的路徑

export const CategoryService = {
    async fetchCategorys() {
        try {
            const response = await getCategories();
            return response;
        } catch (error) {
            console.error("Error fetching categories:", error); // 日誌錯誤
            throw new Error("Failed to fetch categories"); // 提供錯誤上下文
        }
    },

    async addCategory(category) {
        try {
             await createCategory(category);
        } catch (error) {
            console.error("Error categories:", error); // 日誌錯誤
            throw new Error("Failed to categories"); // 提供錯誤上下文
        }
    },

    async modifyCategory (category) {
        try {
            await updateCategory  (category);
        } catch (error) {
            console.error("Error categories:", error); // 日誌錯誤
            throw new Error("Failed to categories"); // 提供錯誤上下文
        }
    },

    async removeCategory (categoryId) {
        try {
            await deleteCategory (categoryId);
        } catch (error) {
            console.error("Error category:", error); // 日誌錯誤
            throw new Error("Failed to categories"); // 提供錯誤上下文
        }
    }
};



