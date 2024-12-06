import {
    getAllProducts,

    updateMainImg,

    editProductApi,

    UpdateAndCreateCarouselsApi,
    getAllCarouselsApi,
    deleteCarouselApi,

    removeSelectedProductDiscountsApi,
    getSelectedDiscountsApi,
    discountProductApi,

    deactivateProductApi,
    activateProductApi,

    CreateAnnouncementApi,
    /*ReadAnnouncementsApi,*/
    UpdateAnnouncementApi,
    deleteAnnouncementApi,



} from '@/api/index';

export const getAllProductsService = {
    ///抓取產品///
    async getProducts() {
        try {
            const data = await getAllProducts(); // 等待資料返回
            return data; // 返回結果的 body
        } catch (err) {
            throw err; // 拋出錯誤
        }
    },

    //////更新產品主圖//////
    async UpdateMainImage(productId, mainImg) {
        return await updateMainImg(productId, mainImg);
    },

    ///產品編輯///
    async updateProduct(product) {
        try {
            const data = await editProductApi(product); // 等待 API 回應
            return data; // 返回結果的 body
        } catch (err) {
            throw err; // 拋出錯誤
        }
    },

    ///產品輪播圖區///
    async UpdateAndCreateCarousels(request) {
        return await UpdateAndCreateCarouselsApi(request);
    },
    async getAllCarousels(productId) {
        return await getAllCarouselsApi(productId);
    },
    async DeleteCarousel(carouselId) {
        return await deleteCarouselApi(carouselId);
    },

    ///產品折扣區///
    async removeSelectedProductDiscounts(selectedDiscountActivity) {
        try {
            console.log(selectedDiscountActivity);
            const data = await removeSelectedProductDiscountsApi(selectedDiscountActivity); // 等待資料返回
            return data; // 返回結果的 body
        } catch (err) {
            throw err; // 拋出錯誤
        }
    },
    async getSelectedDiscounts(productIds) {
        try {
            const data = await getSelectedDiscountsApi(productIds); // 等待資料返回
            return data; // 返回結果的 body
        } catch (err) {
            throw err; // 拋出錯誤
        }
    },
    async discountProduct(discountData) {
        console.log(discountData);
        try {
            const data = await discountProductApi(discountData); // 等待 API 回應
            return data; // 返回結果的 body
        } catch (err) {
            throw err; // 拋出錯誤
        }
    },

    ///產品上下架停用區///
    async deactivateProduct(product) {
        try {
            const data = await deactivateProductApi(product);
            return data;
        } catch (err) {
            throw err;
        }
    },
    async activateProduct(product) {
        try {
            const data = await activateProductApi(product);
            return data;
        } catch (err) {
            throw err;
        }
    },

    ///產品公告區///
    async CreateEvent(event) {
        return await CreateAnnouncementApi(event);
    },
    /*async ReadEvents(productId) {
        return await ReadAnnouncementsApi(productId);
    },*/
    async UpdateEvent(event) {
        return await UpdateAnnouncementApi(event);
    },
    async DeleteEvent(eventid) {
        return await deleteAnnouncementApi(eventid);
    }
};
