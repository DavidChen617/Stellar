import ApiRequest from '@/utilities/ApiRequest';

const api = {
    login: '/api/Auth/Login',
    getAllShippers: '/api/Shipper/GetAllShippers',
    getCategories: '/api/Category/GetCategories',
    createCategory: '/api/Category/CreateCategory',
    updateCategory: '/api/Category/UpdateCategory',
    deleteCategory: (categoryId) => `/api/Category/DeleteCategory/${categoryId}`,
    getTags: '/api/Tag/GetTags',
    createTag: '/api/Tag/CreateTag',
    updateTag: '/api/Tag/UpdateTag',
    deleteTag: (tagId) => `/api/tag/DeleteTag/${tagId}`,
    updateShipper: '/api/Shipper/UpdateShipper',
    getAllProducts: '/api/Product/GetAllProduct',
    getAllUser: '/api/User/getAllUser',
    getAllPublishers: '/api/Publisher/GetAllPublisher',
    updatePublisher: '/api/Publisher/GetAllPublisher',
    getCharts: '/api/Charts/GetChartsData',
    getCategorys: '/api/Categorys',
    getAllCategories: '/api/Product/GetAllCategories',
    getAllTags: '/api/Product/GetAllTags'
};

/*這裡是上課的範例*/
export function login(request) {
    return ApiRequest.httpPost(api.login, request);
}
export function getAllShippers() {
    return ApiRequest.httpGet(api.getAllShippers);
}
export function updateShipper(shipperId, companyName, phone) {
    const request = {
        shipperId: shipperId,
        companyName: companyName,
        phone: phone
    };
    return ApiRequest.httpPut(api.updateShipper, request);
}
/**/

///*這裡才是Stellar的抓取產品*///
export function getAllProducts() {
    return ApiRequest.httpGet(api.getAllProducts)
        .then((response) => {
            //console.log("Raw API Response:", response);  // 原始API
            return response;
        })
        .catch((err) => {
            console.error('API Error:', err); // 捕捉錯誤
        });
}
///新增產品///
export function createProductApi(product) {
    return ApiRequest.httpPost(`/api/Product/CreateProduct`, product);
}

///更新產品主圖///
export function updateMainImg(productId, mainImg) {
    const formData = new FormData();
    formData.append('productId', productId); // 添加 productId 到表單數據
    formData.append('mainImg', mainImg);
    return ApiRequest.httpPost(`/api/Product/UpdateMainImg`, formData);
}

///編輯產品///
export function editProductApi(product) {
    return ApiRequest.httpPost(`/api/Product/UpdateProduct`, product);
}

///產品輪播圖區///
export function UpdateAndCreateCarouselsApi(request) {
    return ApiRequest.httpPost(`/api/Product/UpdateAndCreateCarousels`, request);
}
export function getAllCarouselsApi(productId) {
    return ApiRequest.httpPost(`/api/Product/ReadAllCarousels`, { productId });
}
export function deleteCarouselApi(carouselId) {
    return ApiRequest.httpDelete(`/api/Product/DeleteCarousel/${carouselId}`);
}

///產品折扣區///
export function removeSelectedProductDiscountsApi(selectedDiscountActivity) {
    return ApiRequest.httpPost(`/api/Product/RemoveSelectedProductDiscounts`, selectedDiscountActivity);
}
export function getSelectedDiscountsApi(productIds) {
    return ApiRequest.httpPost(`/api/Product/GetSelectedProductDiscount`, productIds);
}
export function discountProductApi(discountData) {
    return ApiRequest.httpPost(`/api/Product/DiscountProduct`, discountData);
}

///產品上下架停用區///
export function deactivateProductApi(products) {
    return ApiRequest.httpPost(`/api/Product/DeactivateProducts`, products);
}
export function activateProductApi(products) {
    return ApiRequest.httpPost(`/api/Product/ActivateProducts`, products);
}

///產品公告區///
export function CreateAnnouncementApi(formData) {
    return ApiRequest.httpPost(`/api/Product/CreateAnnouncement`, formData);
}
/*export function ReadAnnouncementsApi(productId) {
    return ApiRequest.httpPost(`/api/Product/ReadAnnouncements`, { productId });
}*/
export function UpdateAnnouncementApi(formData) {
    return ApiRequest.httpPost(`/api/Product/UpdateAnnouncement`, formData);
}
export function deleteAnnouncementApi(eventid) {
    return ApiRequest.httpDelete(`/api/Product/DeleteAnnouncement/${eventid}`);
}

///使用者區///
export function getAllUser() {
    return ApiRequest.httpGet(api.getAllUser);
}
export function updateUserApi(userId, islock) {
    return ApiRequest.httpPut(`/api/User/updateUser/${userId}/${islock}`);
}
export function editUserApi(userid, nickName, email, country) {
    return ApiRequest.httpPost(`/api/User/editUser/${userid}/${nickName}/${email}/${country}`);
}

///供應商區///
export function getAllPublishers() {
    return ApiRequest.httpGet(api.getAllPublishers);
}
export function editPublisherApi(publisher) {
    return ApiRequest.httpPost(`/api/Publisher/UpdatePublisher`, publisher);
}

///抓取類別及標籤區(上架產品用)///
export function getAllCategories() {
    return ApiRequest.httpGet(api.getAllCategories);
}
export function getAllTags() {
    return ApiRequest.httpGet(api.getAllTags);
}

///產品類別區///
export function getCategories() {
    return ApiRequest.httpGet(api.getCategories);
}
export function createCategory(category) {
    return ApiRequest.httpPost(api.createCategory, category);
}
export function updateCategory(category) {
    return ApiRequest.httpPut(api.updateCategory, category);
}
export function deleteCategory(categoryId) {
    return ApiRequest.httpDelete(api.deleteCategory(categoryId));
}

///產品標籤區///
export function getTags() {
    return ApiRequest.httpGet(api.getTags);
}
export function createTag(tag) {
    return ApiRequest.httpPost(api.createTag, tag);
}
export function updateTag(tag) {
    return ApiRequest.httpPut(api.updateTag, tag);
}
export function deleteTag(tagId) {
    return ApiRequest.httpDelete(api.deleteTag(tagId));
}

///圖表區///
export function getCharts() {
    return ApiRequest.httpGet(api.getCharts);
}
