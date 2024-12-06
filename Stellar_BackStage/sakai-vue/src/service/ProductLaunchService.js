import { getAllCategories, getAllTags, createProductApi } from "@/api/index";

export const ProductLaunchService = {
    async getCategories() {
        try {
            const data = await getAllCategories();
            return data;
        } catch (err) {
            throw err;
        }
    },
    async getTags() {
        try {
            const data = await getAllTags();
            return data;
        } catch (err) {
            throw err;
        }
    },
    async createProduct(productData) {
        try {
            // 初始化 FormData
            const product = new FormData();
            // 產品訊息
            product.append('ProductDto.ProductName', productData.ProductName);
            product.append('ProductDto.ProductPrice', productData.ProductPrice);
            product.append('ProductDto.ProductShelfTime', productData.ProductShelfTime);
            product.append('ProductDto.Description', productData.Description);
    
            // 發行商ID
            if (productData.PublisherID) {
                product.append('ProductDto.PublisherID', productData.PublisherID);
            }
    
            // 類別ID
            if (productData.CategoryId) {
                product.append('ProductDto.CategoryId', productData.CategoryId);
            }
    
            // 系統需求
            product.append('ProductDto.SystemRequirement', productData.SystemRequirement);
            
            // 產品主圖
            if (productData.ProductMainImageFile) {
                product.append('ProductDto.MainImgFile', productData.ProductMainImageFile);
            }
    
            // 輪播圖片
            if (productData.carouselImages && productData.carouselImages.length > 0) {
                productData.carouselImages.forEach((fileData, index) => {
                    product.append(`CarouselDtos[${index}].CarouselImages`, fileData.file);
                    product.append(`CarouselDtos[${index}].Sort`, fileData.index);
                });
            }
    
            // 標籤ID
            if (productData.tags && productData.tags.length > 0) {
                productData.tags.forEach((tag) => {
                    product.append('TagIds', tag.tagId);
                });
            }
    
            // 公告事件
            if (productData.events && productData.events.length > 0) {
                productData.events.forEach((event, index) => {
                    product.append(`eventDtos[${index}].Title`, event.Title);
                    product.append(`eventDtos[${index}].AnnounceText`, event.AnnounceText);
                    product.append(`eventDtos[${index}].Content`, event.Content);
                    product.append(`eventDtos[${index}].AnnouncementDate`, event.AnnouncementDate ? event.AnnouncementDate : '');
                    if (event.ProductImgFile) {
                        product.append(`eventDtos[${index}].ProductImgFile`, event.ProductImgFile);
                    }
                });
            }
    
            // 關於此遊戲
            product.append('aboutDto.AboutMainTitle', productData.abouts.AboutMainTitle);
    
            // 關於遊戲卡片資訊
            if (productData.abouts.aboutCardListDtos && productData.abouts.aboutCardListDtos.length > 0) {
                productData.abouts.aboutCardListDtos.forEach((about, index) => {
                    product.append(`aboutCardListDtos[${index}].Title`, about.Title);
                    product.append(`aboutCardListDtos[${index}].Text`, about.Text);
                    if (about.AboutCardImgFile) {
                        product.append(`aboutCardListDtos[${index}].AboutCardImgFile`, about.AboutCardImgFile);
                    }
                });
            }
    
            // 發送 POST 請求到後端 API
            await createProductApi(product);

        } catch (error) {
            throw error; // 處理錯誤
        }
    }
};