<script setup>
import { ref, onMounted, computed, reactive, nextTick, watch } from 'vue';
import { FilterMatchMode, FilterOperator } from '@primevue/core/api';
import { useToast } from 'primevue/usetoast';
import { getAllProductsService } from '@/service/AllProductService';
import Sortable from 'sortablejs';
import { useRoute } from 'vue-router';

onMounted(() => {
    resetFilterType(); // 頁面加載時重置篩選條件
    getAllProductsService
        .getProducts()
        .then((data) => {
            console.log('API Response:', data);
            products.value = data;
        })
        .catch((err) => {
            console.error('API Error:', err);
        });
    nextTick(() => {
        const previewContainer = document.getElementById('media-preview');
        if (previewContainer) {
            Sortable.create(previewContainer, {
                animation: 150,
                ghostClass: 'sortable-ghost',
                handle: '.media-item-carousel',
                onEnd: updateMediaOrder
            });
        }
    });
    sortField.value = route.query.sortBy || 'productId';
    sortOrder.value = route.query.order === 'desc' ? -1 : 1;
});

const toast = useToast();
const productDialog = ref(false);
const products = ref(null);
const product = ref({});
const submitted = ref(false);
const discountProductsDialog = ref(false);
const discountActivities = ref([]);
const deleteDiscountDialog = ref(false);
const activityToDelete = ref(null);
const discountStartDate = ref('');
const discountEndDate = ref('');
const discountValue = ref('');
const deactivatedProductDialog = ref(false);
const deactivatedProductsDialog = ref(false);
const activatedProductDialog = ref(false);
const activatedProductsDialog = ref(false);
const deleteProductDialog = ref(false);
const deleteProductsDialog = ref(false);
const selectedProducts = ref();
const filters = ref({
    global: { value: null, matchMode: FilterMatchMode.CONTAINS }
});
const loading = ref(false); // 定義 loading 狀態
const route = useRoute();

const discountSetMinDate = computed(() => getFormattedToday());
const adjustHeight = (event) => {
    const textarea = event.target;
    textarea.style.height = 'auto'; 
    textarea.style.height = `${textarea.scrollHeight}px`; 
};

// 計算今天的日期並格式化為 YYYY-MM-DD
const getFormattedToday = () => {
    const today = new Date();
    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, '0'); // 月份從0開始
    const day = String(today.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`; // 格式化為 YYYY-MM-DD
};

//折扣最大值為100
const validateDiscountInput = () => {
    const value = parseInt(discountValue.value);
    if (isNaN(value) || value < 0 || value > 100) {
        discountValue.value = '';
        toast.error('請輸入0-100的有效數字');
    }
};

//刪除折扣活動modal
function confirmDeleteDisocuntActivity(index) {
    activityToDelete.value = index;
    deleteDiscountDialog.value = true;
}

// async function removeDiscountActivity() {
//     if (activityToDelete.value !== null) {
//         const discountToRemove = discountActivities.value[activityToDelete.value];

//         // 構建要發送的資料
//         const payload = {
//             productId: discountToRemove.productId,
//             discountStartDate: discountToRemove.discountStartDate,
//             discountEndDate: discountToRemove.discountEndDate,
//             discountValue: discountToRemove.discountValue
//         };

//         try {
//             // 請求後端刪除折扣信息
//             await getAllProductsService.removeSelectedProductDiscounts(payload);

//             // 前端刪除折扣活動
//             discountActivities.value.splice(activityToDelete.value, 1);

//             // 根據 productId 重新獲取該產品的剩餘折扣資料
//             const updatedDiscounts = await getAllProductsService.getSelectedDiscounts([discountToRemove.productId]);

//             // 找出剩餘折扣中最大的折扣值
//             const maxDiscount = updatedDiscounts.reduce((max, discount) => Math.max(max, discount.discountValue), 0);

//             // 更新產品資料中的折扣
//             products.value = products.value.map((product) => {
//                 if (product.productId === discountToRemove.productId) {
//                     return { ...product, productDiscount: maxDiscount > 0 ? `- ${maxDiscount}%` : '' };
//                 }
//                 return product;
//             });

//             // 隱藏模態框
//             await fetchdata();
//             deleteDiscountDialog.value = false;
//         } catch (error) {
//             console.error('刪除折扣活動時發生錯誤:', error);
//             // 可以在這裡添加錯誤處理，例如顯示通知
//         }
//     }
// }
async function removeDiscountActivity() {
    if (activityToDelete.value !== null) {
        const discountToRemove = discountActivities.value[activityToDelete.value];

        // 構建要發送的資料
        const payload = {
            productId: discountToRemove.productId,
            discountStartDate: discountToRemove.discountStartDate,
            discountEndDate: discountToRemove.discountEndDate,
            discountValue: discountToRemove.discountValue
        };

        try {
            // 請求後端刪除折扣信息
            await getAllProductsService.removeSelectedProductDiscounts(payload);
            
            // 前端刪除折扣活動
            discountActivities.value.splice(activityToDelete.value, 1);

            // 隱藏模態框
            deleteDiscountDialog.value = false;

            // 獲取更新後的產品資料
            fetchdata();
        } catch (error) {
            console.error('刪除折扣活動時發生錯誤:', error);
            // 錯誤處理
        }
    }
}

// function openNew() {
//     product.value = {};
//     submitted.value = false;
//     productDialog.value = true;
// }

async function editProduct(data) {
    product.value = { ...data };
    productDialog.value = true;
}

function hideDialog() {
    productDialog.value = false;
    submitted.value = false;
    discountProductsDialog.value = false;
}
//渲染取得資料
async function fetchProducts() {
    try {
        const response = await getAllProductsService.getProducts();
        product.value = response;
    } catch (error) {
        console.error('無法獲取資料', error);
    }
}

async function saveProduct() {
    submitted.value = true;
    //todo 串接API
    const payload = { ...product.value };

    if (payload.productId && payload.productName && payload.productPrice && payload.productMainDescription) {
        try {
            await getAllProductsService.updateProduct(payload);

            // 使用 Vue reactivity 即時更新
            const index = products.value.findIndex((p) => p.productId === payload.productId);
            if (index !== -1) {
                products.value[index] = { ...payload }; // 使用新的產品資料更新對應項目
            }
            await fetchdata();
            productDialog.value = false;
            toast.add({ severity: 'success', summary: '更新成功', detail: '產品已更新', life: 3000 });
        } catch (error) {
            console.error('更新失敗', error);
            toast.add({ severity: 'error', summary: '更新失敗', detail: '無法更新產品', life: 3000 });
        }
    }
    // else {
    //     try {
    //         await getAllProductsService.createProduct(product.value); // 呼叫新增產品的API

    //         // 新增到產品列表中
    //         products.value.push({ ...product.value });

    //         productDialog.value = false;
    //         toast.add({ severity: 'success', summary: '新增成功', detail: '產品已新增', life: 3000 });
    //     } catch (error) {
    //         console.error('新增失敗', error);
    //         toast.add({ severity: 'error', summary: '新增失敗', detail: '無法新增產品', life: 3000 });
    //     }
    // }
}
//-----------------------------------------------------
//多選制定折扣紐
async function confirmDiscountSelected() {
    try {
        // 取得選中的 productId
        const productIds = selectedProducts.value.map((product) => product.productId);

        // 請求後端以獲取折扣信息
        const discounts = await getAllProductsService.getSelectedDiscounts(productIds);

        // 將折扣信息保存到 discountActivities
        discountActivities.value = discounts;

        // 清除折扣起始日、結束日和折扣值
        discountStartDate.value = ''; // 重置起始日
        discountEndDate.value = ''; // 重置結束日
        discountValue.value = ''; // 重置折扣值

        // 打開對話框
        fetchdata();
        discountProductsDialog.value = true;
    } catch (error) {
        console.error('獲取折扣信息時發生錯誤:', error);
    }

    // discountProductsDialog.value = true;
}
//制定折扣多選
async function discountSelectedProducts() {
    try {
        // 準備要傳給後端的資料
        const discountData = selectedProducts.value.map((product) => ({
            productId: product.productId,
            discountStartDate: discountStartDate.value,
            discountEndDate: discountEndDate.value,
            discountValue: discountValue.value
        }));

        // 傳送多個 product 的折扣資料給後端
        await getAllProductsService.discountProduct(discountData);

        selectedProducts.value = [];
        // 關閉對話框
        fetchdata();
        discountProductsDialog.value = false;

        // 顯示成功通知
        toast.add({ severity: 'success', summary: 'Successful', detail: 'Products discounted successfully', life: 3000 });
    } catch (error) {
        console.error('制定折扣多個產品時發生錯誤:', error);
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to apply discount to products', life: 3000 });
    }
}
//-----------------------------------------------------

// 下架鈕
function confirmDeactivatedProduct(prod) {
    product.value = prod;
    deactivatedProductDialog.value = true;
}
//多選下架鈕
function confirmDeactivatedSelected() {
    deactivatedProductsDialog.value = true;
}
//下架單選
async function deactivatedProduct() {
    try {
        // 呼叫 API 將產品狀態改為已下架
        await getAllProductsService.deactivateProduct([product.value.productId]);

        // 更新本地的產品狀態
        const updatedProduct = { ...product.value, productStatus: 2 };

        products.value = products.value.map((val) => (val.productId === product.value.productId ? updatedProduct : val));

        // 關閉對話框
        deactivatedProductDialog.value = false;

        // 重置產品值
        product.value = {};

        // 顯示成功的通知
        toast.add({ severity: 'success', summary: 'Successful', detail: 'Product deactivated', life: 3000 });
        //console.log('準備下架product!', updatedProduct);
    } catch (error) {
        // 處理錯誤情況
        console.error('下架產品時發生錯誤:', error);
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to deactivate product', life: 3000 });
    }
}

//下架多選
async function deactivatedSelectedProducts() {
    try {
        const selectedProductIds = selectedProducts.value.map((product) => product.productId);

        // 傳送多個 productId 給後端
        await getAllProductsService.deactivateProduct(selectedProductIds);

        products.value = products.value.map((product) => (selectedProductIds.includes(product.productId) ? { ...product, productStatus: 2 } : product));
        selectedProducts.value = [];
        // 關閉對話框
        deactivatedProductsDialog.value = false;

        // 顯示成功通知
        toast.add({ severity: 'success', summary: 'Successful', detail: 'Products deactivated', life: 3000 });
    } catch (error) {
        console.error('下架多個產品時發生錯誤:', error);
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to deactivate products', life: 3000 });
    }
}
//----------------------------------------------------

// 上架鈕
function confirmActivatedProduct(prod) {
    product.value = prod;
    activatedProductDialog.value = true;
}
//多選上架鈕
function confirmActivatedSelected() {
    activatedProductsDialog.value = true;
}
//上架單選
async function activatedProduct() {
    try {
        // 呼叫 API 將產品狀態改為已下架
        await getAllProductsService.activateProduct([product.value.productId]);

        // 更新本地的產品狀態
        const updatedProduct = { ...product.value, productStatus: 1 };

        products.value = products.value.map((val) => (val.productId === product.value.productId ? updatedProduct : val));

        // 關閉對話框
        activatedProductDialog.value = false;

        // 重置產品值
        product.value = {};

        // 顯示成功的通知
        toast.add({ severity: 'success', summary: 'Successful', detail: 'Product activated', life: 3000 });
        //console.log('準備上架product!', updatedProduct);
    } catch (error) {
        // 處理錯誤情況
        console.error('上架產品時發生錯誤:', error);
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to activate product', life: 3000 });
    }
}

//上架多選
async function activatedSelectedProducts() {
    try {
        const selectedProductIds = selectedProducts.value.map((product) => product.productId);

        // 傳送多個 productId 給後端
        await getAllProductsService.activateProduct(selectedProductIds);

        products.value = products.value.map((product) => (selectedProductIds.includes(product.productId) ? { ...product, productStatus: 1 } : product));
        selectedProducts.value = [];
        // 關閉對話框
        activatedProductsDialog.value = false;

        // 顯示成功通知
        toast.add({ severity: 'success', summary: 'Successful', detail: 'Products activated', life: 3000 });
    } catch (error) {
        console.error('下架多個產品時發生錯誤:', error);
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to activate products', life: 3000 });
    }
}

//-----------------------------------------------------
// 刪除鈕
function confirmDeleteProduct(prod) {
    product.value = prod;
    deleteProductDialog.value = true;
}

// 刪除
function deleteProduct() {
    products.value = products.value.filter((val) => val.productId !== product.value.productId);
    deleteProductDialog.value = false;
    product.value = {};
    toast.add({ severity: 'success', summary: 'Successful', detail: 'Product Deleted', life: 3000 });
    //todo 串接API
    //console.log(product.value);
}

function deleteSelectedProducts() {
    products.value = products.value.filter((val) => !selectedProducts.value.includes(val));
    deleteProductsDialog.value = false;
    selectedProducts.value = null;
    toast.add({ severity: 'success', summary: 'Successful', detail: 'Products Deleted', life: 3000 });
    //刪除資料庫
}

function confirmDeleteSelected() {
    deleteProductsDialog.value = true;
}
// -----------------------------------------------
//取得狀態
function getStatusLabel(status) {
    switch (status) {
        case true:
            return 'success';

        case false:
            return 'danger';

        default:
            return null;
    }
}

function formatCurrency(value) {
    if (value) {
        const roundedValue = Math.round(value);
        const formattedValue = roundedValue.toLocaleString('zh-TW', {
            style: 'currency',
            currency: 'TWD',
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        });
        return formattedValue;
    }
    return;
}
//下架狀態抓取
function getProductStatus(status) {
    switch (status) {
        case 0:
            return '待審核';
        case 1:
            return '上架中';
        case 2:
            return '已下架';
        default:
            return '待審核';
    }
}
function getProductStatusLabel(status) {
    switch (status) {
        case 0:
            return 'warning';
        case 1:
            return 'success';
        case 2:
            return 'danger';
        default:
            return 'warning';
    }
}

// 狀態管理
const carouselDialog = ref(false);
const carouselItems = ref([]);
const previewContainer = ref(null);
let sortableInstance = null;

watch(carouselDialog, async (newVal) => {
    if (newVal) {
        await nextTick();
        if (previewContainer.value) {
            // 銷毀已有的 Sortable 實例（如果有）
            if (sortableInstance) {
                sortableInstance.destroy();
                sortableInstance = null;
            }
            // 創建新的 Sortable 實例
            sortableInstance = Sortable.create(previewContainer.value, {
                animation: 150,
                ghostClass: 'sortable-ghost',
                handle: '.media-item-carousel',
                onEnd: updateMediaOrder
            });
        }
    } else {
        if (sortableInstance) {
            sortableInstance.destroy();
            sortableInstance = null;
        }
    }
});

// 開啟輪播圖 Modal
async function openCarouselDialog(productData) {
    if (productData && productData.productId) {
        product.value = { ...productData };
        try {
            const readcarousel = await getAllProductsService.getAllCarousels(productData.productId);
            carouselItems.value = readcarousel.map((item, index) => ({
                ...item,
                sort: item.sort !== undefined ? item.sort : index,
                isNew: false,
                isDeleted: false
            }));
            carouselDialog.value = true; // 打開對話框
        } catch (error) {
            console.error('獲取輪播圖片時發生錯誤:', error);
            toast.add({ severity: 'error', summary: '錯誤', detail: '無法獲取輪播圖片', life: 3000 });
        }
    } else {
        toast.add({ severity: 'error', summary: '錯誤', detail: 'Product ID 不存在或無效', life: 3000 });
    }
}

// 關閉輪播圖 Modal
function closeCarouselDialog() {
    carouselDialog.value = false;
    carouselItems.value = [];
}

// 處理文件變更
function handleFileChange(event) {
    const fileList = event.target.files;
    Array.from(fileList).forEach((file, index) => {
        const reader = new FileReader();
        reader.onload = (e) => {
            if (file.type.startsWith('video/')) {
                addVideoItem(e.target.result, file);
            } else if (file.type.startsWith('image/')) {
                addImageItem(e.target.result, file);
            }
        };
        reader.readAsDataURL(file);
    });
}

// 生成檔名的方法
function getFileName(url) {
    const parts = url.split('/');
    return parts[parts.length - 1].split('?')[0]; // 去除 URL 中的參數
}

function generateItemKey(item, index) {
    if (item && item.carouselId) {
        return item.carouselId; // 使用現有的 carouselId 作為鍵
    } else if (item && item.file) {
        return 'new-' + item.file.name + '-' + Date.now(); // 為新項目生成唯一鍵
    } else {
        return 'undefined-' + index;
    }
}

// 添加影片
function addVideoItem(dataSourceUrl, file) {
    carouselItems.value.push({
        type: 0, // 0 表示影片
        dataSourceUrl,
        preview: '',
        thumbnail: '',
        isDeleted: false,
        isNew: true,
        file // 儲存File
    });
}

// 添加圖片
function addImageItem(dataSourceUrl, file) {
    carouselItems.value.push({
        type: 1, // 1 表示圖片
        dataSourceUrl,
        preview: '',
        thumbnail: '',
        isDeleted: false,
        isNew: true,
        file // 儲存File
    });
}

// 在刪除文件後重新給文件排序
function updateMediaOrder(evt) {
    if (!carouselItems.value || !Array.isArray(carouselItems.value)) {
        console.error('carouselItems.value 未定義或不是陣列');
        return;
    }
    const movedItem = carouselItems.value.splice(evt.oldIndex, 1)[0];
    carouselItems.value.splice(evt.newIndex, 0, movedItem);
    updateSortValues();
}

// 更新排序值
function updateSortValues() {
    carouselItems.value.forEach((item, index) => {
        item.sort = index;
    });
}

// 生成影片縮略圖
function generateThumbnail(event, index) {
    const video = event.target;
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');

    // 設置 canvas 尺寸與影片相同
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;

    // 繪製當前影片到 canvas
    ctx.drawImage(video, 0, 0, canvas.width, canvas.height);

    // 確保 carouselItems.value[index] 存在
    const thumbnail = canvas.toDataURL('image/png');

    // 确保 carouselItems.value[index] 存在
    if (carouselItems.value[index]) {
        // 將縮圖存在對應的 carouselItems 中
        carouselItems.value[index].thumbnail = thumbnail;
    }
}

// 保存輪播圖
async function saveCarousel() {
    if (!product.value.productId) {
        toast.add({ severity: 'error', summary: '錯誤', detail: 'Product ID 未定義', life: 3000 });
        return;
    }

    updateSortValues();

    // 確保 carouselItems.value 已定義且為陣列
    if (!carouselItems.value || !Array.isArray(carouselItems.value)) {
        carouselItems.value = [];
    }

    // 過濾掉已刪除的項目
    const itemsToSave = carouselItems.value.filter((item) => !item.isDeleted);

    // 確保 itemsToSave 為陣列
    if (!itemsToSave || !Array.isArray(itemsToSave)) {
        console.error('itemsToSave 未定義或不是陣列');
        return;
    }

    // // 準備要上傳的新圖片
    const newImagesFormData = new FormData();
    itemsToSave.forEach((item, index) => {
        newImagesFormData.append(`request[${index}].CarouselId`, item.carouselId);
        newImagesFormData.append(`request[${index}].ProductID`, product.value.productId);
        newImagesFormData.append(`request[${index}].CarouselImages`, item.file); // 文件
        newImagesFormData.append(`request[${index}].Sort`, index);
    });
    // console.log('newImagesFormData 內容:');
    // for (let pair of newImagesFormData.entries()) {
    //     console.log(`${pair[0]}:`, pair[1]);
    // }

    try {
        loading.value = true;
        await getAllProductsService.UpdateAndCreateCarousels(newImagesFormData);

        toast.add({ severity: 'success', summary: '成功', detail: '輪播圖已更新', life: 3000 });
        closeCarouselDialog();
        loading.value = false;
        carouselItems.value = []; // 清空
    } catch (error) {
        toast.add({ severity: 'error', summary: '失敗', detail: '輪播圖更新失敗', life: 3000 });
        loading.value = false;
    }
}

async function removeCarouselItem(index) {
    const item = carouselItems.value[index];
    if (item.carouselId) {
        try {
            await getAllProductsService.DeleteCarousel(item.carouselId);
            carouselItems.value.splice(index, 1);
        } catch (error) {
            toast.add({ severity: 'error', summary: '錯誤', detail: '刪除圖片失敗', life: 3000 });
        }
    } else {
        carouselItems.value.splice(index, 1);
    }
    updateSortValues();
}

const selectedProduct = ref({ productAnnouncements: [] }); // 選中的產品公告
const announcementDialog = ref(false);
const filterType = ref('threeMonths'); // 預設篩選為最近三個月
const editDialog = ref(false);
const modalTitle = ref('');
const selectedAnnouncement = ref({
    title: '',
    announceText: '',
    announcementDate: new Date(),
    content: '',
    productImgUrl: ''
});
const selectedFile = ref(null);

// 打開新增公告模態框
function openCreateAnnouncementModal() {
    modalTitle.value = '新增公告';
    selectedAnnouncement.value = {
        title: '',
        announceText: '',
        announcementDate: new Date(),
        content: '',
        productImgUrl: ''
    };
    selectedFile.value = null;
    editDialog.value = true;
}

// 重置篩選條件為 "三個月"
function resetFilterType() {
    filterType.value = 'threeMonths';
}

// 使用計算屬性來篩選和排序公告
const sortedFilteredAnnouncements = computed(() => {
    if (selectedProduct.value.productAnnouncements) {
        const sorted = selectedProduct.value.productAnnouncements.sort(
            (a, b) => new Date(b.announcementDate) - new Date(a.announcementDate) // 最近日期排在最前
        );
        return sorted.filter((announcement) => {
            const announcementDate = new Date(announcement.announcementDate);
            if (filterType.value === 'threeMonths') {
                const threeMonthsAgo = new Date();
                threeMonthsAgo.setMonth(threeMonthsAgo.getMonth() - 3);
                return announcementDate >= threeMonthsAgo;
            } else if (filterType.value === 'oneYear') {
                const oneYearAgo = new Date();
                oneYearAgo.setFullYear(oneYearAgo.getFullYear() - 1);
                return announcementDate >= oneYearAgo;
            } else {
                return true;
            }
        });
    }
    return [];
});

function editProductAnnouncement(product) {
    product.productAnnouncements.forEach((announcement) => {
        normalizeDateFormat(announcement); // 確保日期格式一致
    });
    selectedProduct.value = product;
    filterType.value = 'threeMonths';
    announcementDialog.value = true;
}
function normalizeDateFormat(announcement) {
    if (!announcement.date) {
        announcement.date = new Date(announcement.announcementDate).toISOString();
    }
}

// 編輯公告功能
function openEditAnnouncementModal(announcement) {
    modalTitle.value = '編輯公告';
    selectedAnnouncement.value = { ...announcement };
    selectedAnnouncement.value.announcementDate = new Date(selectedAnnouncement.value.announcementDate);
    selectedFile.value = null;
    editDialog.value = true;
}

async function deleteAnnouncement(announcement, index) {
    if (confirm(`確定要刪除公告「${announcement.title}」嗎？`)) {
        selectedProduct.value.productAnnouncements.splice(index, 1);
        var eventid = announcement.eventsId;
        // 更新後端數據
        try {
            const response = await getAllProductsService.DeleteEvent(eventid);
            //console.log('Delete response:', response);
        } catch (error) {
            console.error('無法獲取資料:', error);
        }
    }
}

// 處理圖片文件變化
const previewEventImageUrl = ref('');

const onFileChange = (event) => {
    selectedFile.value = event.target.files[0];
    const file = event.target.files[0];
    if (file && (file.type === 'image/jpeg' || file.type === 'image/png' || file.type === 'image/webp')) {
        const reader = new FileReader();
        reader.onload = (e) => {
            previewEventImageUrl.value = e.target.result; // 只更新預覽圖，不覆蓋原本的圖片
        };
        reader.readAsDataURL(file);
    }
};

// 日期格式化函數
function formatDateToYYYYMMDD(date) {
    const d = new Date(date);
    const year = d.getFullYear();
    const month = (d.getMonth() + 1).toString().padStart(2, '0'); // 月份從0開始，故+1，並補0
    const day = d.getDate().toString().padStart(2, '0'); // 補0
    return `${year}-${month}-${day}`; // 返回 yyyy-MM-dd 格式
}

// 保存公告
async function saveAnnouncement() {
    const requiredFields = ['title', 'announceText', 'announcementDate', 'content'];

    const isAnyFieldEmpty = requiredFields.some((field) => !selectedAnnouncement.value[field]);

    if (isAnyFieldEmpty) {
        toast.add({ severity: 'error', summary: '失敗', detail: '請填寫所有必填字段', life: 3000 });
        return;
    }

    const formData = new FormData();
    formData.append('ProductId', selectedProduct.value.productId);
    formData.append('Title', selectedAnnouncement.value.title);
    formData.append('AnnounceText', selectedAnnouncement.value.announceText);
    formData.append('AnnouncementDate', formatDateToYYYYMMDD(selectedAnnouncement.value.announcementDate));
    formData.append('Content', selectedAnnouncement.value.content);
    if (selectedFile.value) {
        formData.append('EventImgUrl', selectedFile.value);
    }

    loading.value = true;

    if (selectedAnnouncement.value.eventsId) {
        formData.append('EventsId', selectedAnnouncement.value.eventsId);
        await getAllProductsService.UpdateEvent(formData);
    } else {
        if (!selectedFile.value) {
            toast.add({ severity: 'error', summary: '新增失敗', detail: '請放公告圖片', life: 3000 });
        } else {
            await getAllProductsService.CreateEvent(formData);
        }
    }
    fetchdata();
    editDialog.value = false;

    // 更新本地公告列表
    if (selectedAnnouncement.value.eventsId) {
        const index = selectedProduct.value.productAnnouncements.findIndex((announcement) => announcement.eventsId === selectedAnnouncement.value.eventsId);
        if (index !== -1) {
            selectedProduct.value.productAnnouncements[index] = { ...selectedAnnouncement.value };
        }
    } else {
        selectedProduct.value.productAnnouncements.push({ ...selectedAnnouncement.value });
    }
    loading.value = false;
}

// 檢查公告日期是否早於今天
function isBeforeToday(announcementDate) {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const announcementDay = new Date(announcementDate);
    return announcementDay < today;
}

// 設置最小日期為今天
const minDate = computed(() => {
    return new Date(); // 返回當前日期作為最小日期
});

//編輯主圖
const ProductsMainImageDialog = ref(false);
const previewImage = ref(null);
const file = ref(null);

function closeMainImgDialog() {
    ProductsMainImageDialog.value = false;
    previewImage.value = null;
}

function onMainImgChange(event) {
    const selectedFile = event.target.files[0];
    if (selectedFile) {
        file.value = selectedFile; // 將檔案存到 file 中
        previewImage.value = URL.createObjectURL(file.value); // 使用 file.value 生成預覽 URL
    }
}
function editProductMainImg(data) {
    previewImage.value = null;
    product.value = { ...data };
    ProductsMainImageDialog.value = true;
}
async function saveMainImg() {
    if (file.value) {
        //todo 串接API
        try {
            loading.value = true;
            await getAllProductsService.UpdateMainImage(product.value.productId, file.value);
            fetchdata();

            loading.value = false;
            ProductsMainImageDialog.value = false;

            toast.add({ severity: 'success', summary: '更新成功', detail: '圖片已更新', life: 3000 });
        } catch (error) {
            console.error('更新失敗', error);
            loading.value = false;
        }
    } else {
        toast.add({ severity: 'error', summary: '更新失敗', detail: '請上傳一張圖片', life: 3000 });
    }
}

async function fetchdata() {
    try {
        products.value = await getAllProductsService.getProducts();
        if (selectedProduct.value.productId) {
            const updatedProduct = products.value.find((p) => p.productId === selectedProduct.value.productId);
            if (updatedProduct) {
                selectedProduct.value = updatedProduct;
            }
        }
    } catch (err) {
        console.error('API Error:', err);
    }
}

// 預設的排序字段和順序
const sortField = ref(route.query.sortBy || 'productId');
const sortOrder = ref(route.query.order === 'desc' ? -1 : 1);
</script>

<template>
    <div className="card">
        <Toolbar class="mb-6">
            <template #start>
                <!-- <Button label="新增" icon="pi pi-plus" severity="secondary" class="mr-2" @click="openNew" /> -->
                <router-link to="/pages/ProductLaunch" class="mr-2 p-button p-component p-button-secondary">
                    <span class="p-button-icon pi pi-plus"></span>
                    <span class="p-button-label">新增產品</span>
                </router-link>
                <!-- <Button label="刪除" icon="pi pi-trash" class="mr-2" severity="secondary" @click="confirmDeleteSelected" :disabled="!selectedProducts || !selectedProducts.length" /> -->

                <Button label="上架" icon="pi pi-calendar-times" class="mr-2" severity="secondary" @click="confirmActivatedSelected" :disabled="!selectedProducts || !selectedProducts.length" />

                <Button label="下架" icon="pi pi-calendar-times" class="mr-2" severity="secondary" @click="confirmDeactivatedSelected" :disabled="!selectedProducts || !selectedProducts.length" />

                <Button label="制定折扣" icon="pi pi-tag" severity="secondary" @click="confirmDiscountSelected" :disabled="!selectedProducts || !selectedProducts.length" />
            </template>
        </Toolbar>

        <!-- ------------------------------------------------------------------------- -->
        <DataTable
            :value="products"
            tableStyle="min-width: 50rem"
            ref="dt"
            v-model:selection="selectedProducts"
            dataKey="productId"
            :paginator="true"
            :rows="10"
            :filters="filters"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
            sort-mode="single"
            :sortField="sortField"
            :sortOrder="sortOrder"
        >
            <template #header>
                <div class="flex flex-wrap gap-2 items-center justify-between">
                    <h4 class="m-0">產品管理</h4>
                    <IconField>
                        <InputIcon>
                            <i class="pi pi-search" />
                        </InputIcon>
                        <InputText v-model="filters['global'].value" placeholder="Search..." />
                    </IconField>
                </div>
            </template>

            <Column selectionMode="multiple" style="width: 3rem" :exportable="false"></Column>

            <Column field="productId" header="ID" sortable style="text-align: center;"></Column>

            <Column field="productStatus" header="上架狀態" sortable style="min-width: 8rem">
                <template #body="slotProps">
                    <div class="flex justify-center">
                        <Tag :value="getProductStatus(slotProps.data.productStatus)" :severity="getProductStatusLabel(slotProps.data.productStatus)" />
                    </div>
                </template>
            </Column>

            <Column header="遊戲圖片" style="min-width: 8rem">
                <template #body="slotProps">
                    <div class="flex flex-row">
                        <img :src="`${slotProps.data.productMainImageUrl}`" :alt="slotProps.data.productMainImageUrl" class="rounded" style="width: 64px; aspect-ratio: 16/9" />
                        <Button outlined rounded class="ms-1" @click="editProductMainImg(slotProps.data)">
                            <i class="pi pi-pencil"></i>
                        </Button>
                    </div>
                </template>
            </Column>
            <Column field="productName" header="遊戲名稱" style="min-width: 5rem"></Column>

            <Column field="productPrice" header="原始售價" style="min-width: 7.5rem" sortable>
                <template #body="slotProps">
                    <span :class="{ 'line-through text-red-600': slotProps.data.productPrice !== 0 && slotProps.data.productPrice !== slotProps.data.productSalesPrice }" v-if="slotProps.data.productPrice !== 0">
                        NT {{ formatCurrency(slotProps.data.productPrice) }}
                    </span>
                    <span class="text-green-600" v-else> 免費 </span>
                </template>
            </Column>

            <Column field="productSalesPrice" header="實際售價" style="min-width: 7.5rem" sortable>
                <template #body="slotProps">
                    <span :class="{ 'text-green-600': slotProps.data.productSalesPrice !== 0 && slotProps.data.productPrice !== slotProps.data.productSalesPrice }" v-if="slotProps.data.productSalesPrice !== 0">
                        NT {{ formatCurrency(slotProps.data.productSalesPrice) }}
                    </span>
                    <span class="text-green-600" v-else> 免費 </span>
                </template>
            </Column>

            <Column field="productDiscount" header="折扣" style="min-width: 6rem" sortable>
                <template #body="slotProps">
                    <span class="text-green-600" v-if="slotProps.data.productDiscount"> {{ slotProps.data.productDiscount }} </span>
                    <span v-else> 無 </span>
                </template>
            </Column>

            <Column field="productCategory" header="類別" style="min-width: 6rem" sortable></Column>

            <Column field="productPublisher" header="供應商" style="min-width: 5rem" sortable></Column>

            <Column field="productShelfTime" header="發行日期" style="min-width: 8rem" sortable></Column>

            <Column field="productSellStatus" header="發行狀態" sortable style="min-width: 8rem; margin: auto">
                <template #body="slotProps">
                    <div class="flex justify-center">
                        <Tag :value="slotProps.data.productSellStatus ? '已發行' : '未發行'" :severity="getStatusLabel(slotProps.data.productSellStatus)" />
                    </div>
                </template>
            </Column>

            <!-- <Column field="productStatus" header="上架狀態" sortable style="min-width: 5rem">
                <template #body="slotProps">
                    <Tag :value="getProductStatus(slotProps.data.productStatus)" :severity="getProductStatusLabel(slotProps.data.productStatus)" />
                </template>
            </Column> -->

            <Column field="Comments" header="評論" :showFilterMatchModes="false" style="min-width: 7rem">
                <template #body="slotProps">
                    <div class="progress-container">
                        <!-- 確認 slotProps 和 slotProps.data 存在 -->
                        <div v-if="slotProps && slotProps.data" class="progress-bar-filler">
                            <!-- 進度條 -->
                            <div class="progress-bar-wrapper">
                                <!-- 好評部分 -->
                                <div class="progress-bar positive" :style="{ width: (slotProps.data.productGoodCommentPercentage || 0) + '%' }"></div>
                                <!-- 差評部分 -->
                                <div class="progress-bar negative" :style="{ width: (slotProps.data.productBadCommentPercentage || 0) + '%' }"></div>
                            </div>
                            <!-- 數字顯示 -->
                            <div class="progress-labels">
                                <span class="good-comment-label">{{ slotProps.data.productGoodCommentPercentage || 0 }}% </span>
                                <span class="bad-comment-label">{{ slotProps.data.productBadCommentPercentage || 0 }}% </span>
                            </div>
                        </div>
                        <div v-else>無評論數據</div>
                    </div>
                </template>
            </Column>

            <!-- 編輯、刪除、下架 -->
            <Column :exportable="false" style="min-width: 13rem">
                <template #body="slotProps">
                    <!-- <Button icon="pi pi-pencil" outlined rounded class="mr-2" @click="editProduct(slotProps.data)" /> -->
                    <Button outlined rounded class="mr-2" @click="editProduct(slotProps.data)">
                        <i class="pi pi-pen-to-square"></i>
                        <span>編輯</span>
                    </Button>
                    <!-- 單個產品折扣先註解 -->
                    <!-- <Button outlined rounded severity="warning" class="mb-2" @click="confirmDiscountProduct(slotProps.data)">
                        <i class="pi pi-tag"></i>
                        <span>制定個別折扣</span>
                    </Button> -->

                    <!-- 單個產品刪除先註解 -->
                    <!-- <Button icon="pi pi-trash" outlined rounded class="mr-2" severity="danger" @click="confirmDeleteProduct(slotProps.data)" /> -->

                    <!-- 單個產品上架及下架先註解 -->
                    <!-- <Button outlined rounded severity="success" class="mr-2" @click="confirmActivatedProduct(slotProps.data)">
                        <i class="pi pi-calendar-plus"></i>
                        <span>上架</span>
                    </Button> -->
                    <!-- <Button outlined rounded severity="danger" @click="confirmDeactivatedProduct(slotProps.data)">
                        <i class="pi pi-calendar-times"></i>
                        <span>下架</span>
                    </Button>
                    <br />-->
                    <Button outlined rounded severity="info" @click="openCarouselDialog(slotProps.data)">
                        <i class="pi pi-images"></i>
                        <span>輪播圖</span>
                    </Button>
                    <Button outlined rounded severity="info" @click="editProductAnnouncement(slotProps.data)" class="mt-2 mr-2">
                        <i class="pi pi-calendar-times"></i>
                        <span>公告</span>
                    </Button>
                    <!-- <Button outlined rounded severity="info" class="mt-2">
                        <i class="pi pi-align-justify"></i>
                        <span>關於這</span>
                    </Button> -->
                </template>
            </Column>
        </DataTable>

        <!--Edit Modal -->
        <Dialog v-model:visible="productDialog" :style="{ width: '450px' }" header="編輯產品" :modal="true">
            <div class="flex flex-col gap-6">
                <div>
                    <label for="productId" class="block font-bold mb-3">ID</label>
                    <InputText id="productId" v-model="product.productId" :invalid="submitted && !product.productId" disabled="true" fluid />
                </div>
                <div>
                    <label for="productName" class="block font-bold mb-3">遊戲名稱</label>
                    <InputText id="productName" v-model.trim="product.productName" required="true" autofocus :invalid="submitted && !product.productName" fluid />
                    <small v-if="submitted && !product.productName" class="text-red-500">必填</small>
                </div>
                <div>
                    <label for="productPrice" class="block font-bold mb-3">價格</label>
                    <InputText id="productPrice" v-model.trim="product.productPrice" required="true" :invalid="submitted && product.productPrice === ''" fluid />
                    <small v-if="submitted && product.productPrice === ''" class="text-red-500">必填</small>
                </div>
                <div>
                    <label for="productMainDescription" class="block font-bold mb-3">描述</label>
                    <textarea id="productMainDescription" v-model="product.productMainDescription" placeholder="請輸入產品描述..." rows="3" class="border border-gray-600 w-full resize-none" @input="adjustHeight"></textarea>
                    <small v-if="submitted && product.productMainDescription === ''" class="text-red-500">必填</small>
                </div>
            </div>

            <template #footer>
                <Button label="取消" icon="pi pi-times" text @click="hideDialog" ></Button>
                <Button label="儲存" icon="pi pi-check" @click="saveProduct" ></Button>
            </template>
        </Dialog>

        <!-- 輪播圖 Modal -->
        <Dialog v-model:visible="carouselDialog" :style="{ width: '75%' }" header="輪播圖" :modal="true">
            <template v-if="product.productId">
                <div class="ps-12">
                    <p>Product ID: {{ product.productId }}</p>
                    <div>
                        <label class="inline-block w-full">遊戲畫面(限10MB以下) 允許多選</label>
                        <input type="file" multiple @change="handleFileChange" accept="image/*" />
                    </div>
                    <div id="media-preview" ref="previewContainer" class="mt-4 flex flex-wrap flex-row">
                        <div v-for="(item, index) in carouselItems" :key="generateItemKey(item, index)">
                            <div v-if="item && !item.isDeleted" class="my-1 w-1/4 media-item-carousel" :data-index="index" :style="{ cursor: 'move' }">
                                <p>{{ index + 1 }}</p>
                                <img
                                    v-if="item.dataSourceUrl.includes('jpeg') || item.dataSourceUrl.includes('webp') || item.dataSourceUrl.includes('gif') || item.dataSourceUrl.includes('jpg')"
                                    :src="item.dataSourceUrl || item.preview"
                                    style="max-width: 200px; aspect-ratio: 16/9"
                                />
                                <video
                                    v-if="item.type === 0 || item.dataSourceUrl.includes('video')"
                                    :src="item.dataSourceUrl || item.preview"
                                    style="max-width: 200px; aspect-ratio: 16/9"
                                    @loadeddata="generateThumbnail($event, index)"
                                    controls
                                ></video>
                                <p v-if="item.dataSourceUrl.includes('cloudinary')" class="w-[200px] break-words">
                                    {{ getFileName(item.dataSourceUrl) }}
                                </p>
                                <p v-else class="w-[200px] break-words">
                                    {{ item.file ? item.file.name : '無檔名' }}
                                </p>
                                <button @click="removeCarouselItem(index)" class="mt-2 bg-red-500 text-white px-2 py-1 rounded">刪除</button>
                            </div>
                        </div>
                    </div>
                </div>
            </template>
            <template #footer>
                <Button label="取消" @click="closeCarouselDialog" class="p-button-secondary"></Button>
                <Button label="保存" @click="saveCarousel" :disabled="carouselItems.length === 0"></Button>
            </template>
        </Dialog>

        <!--公告Modal -->
        <Dialog v-model:visible="announcementDialog" :style="{ width: '600px' }" header="產品公告" :modal="true">
            <label for="filter-select" class="block mb-2">
                篩選公告:
                <select id="filter-select" v-model="filterType" class="border border-gray-300 rounded p-2">
                    <option value="threeMonths">最近三個月</option>
                    <option value="oneYear">最近一年</option>
                    <option value="all">全部</option>
                </select>
                <Button label="新增公告" icon="pi pi-plus" @click="openCreateAnnouncementModal" class="float-right" />
            </label>
            <div v-if="sortedFilteredAnnouncements.length > 0" class="flex overflow-x-auto gap-4 p-4">
                <div v-for="(announcement, index) in sortedFilteredAnnouncements" :key="index" class="min-w-[300px] shadow-md rounded-lg p-4">
                    <h3 class="text-lg font-bold mb-2">{{ announcement.title }}</h3>
                    <div>
                        <img :src="announcement.productImgUrl" alt="公告圖片" class="mb-2 w-full h-auto rounded aspect-video object-cover" />
                        <p class="text-gray-600">{{ announcement.announceText }}</p>
                        <p class="text-gray-700">{{ announcement.content }}</p>
                        <small class="text-gray-500">{{ new Date(announcement.announcementDate).toLocaleDateString() }}</small>
                    </div>
                    <div class="flex justify-end mt-4">
                        <Button icon="pi pi-pencil" label="編輯" @click="openEditAnnouncementModal(announcement)" :disabled="isBeforeToday(announcement.announcementDate)" v-tooltip="'今天以前不可編輯'" />
                        <Button icon="pi pi-trash" severity="danger" label="刪除" @click="deleteAnnouncement(announcement, index)" />
                    </div>
                </div>
            </div>
            <div v-else>
                <p>此產品目前沒有公告。</p>
            </div>
            <template #footer>
                <Button label="關閉" icon="pi pi-times" text @click="announcementDialog = false" />
            </template>
        </Dialog>

        <!-- 新增/編輯公告模態框 -->
        <Dialog v-model:visible="editDialog" :style="{ width: '600px' }" :header="modalTitle" :modal="true">
            <div>
                <div class="mb-4">
                    <label class="block font-bold mb-2">公告標題</label>
                    <InputText v-model="selectedAnnouncement.title" placeholder="限制50字元" class="w-full p-2 border rounded" required />
                </div>
                <div class="mb-4">
                    <label class="block font-bold mb-2">預告詞</label>
                    <InputText v-model="selectedAnnouncement.announceText" placeholder="限制50字元" class="w-full p-2 border rounded" required />
                </div>
                <div class="mb-4">
                    <label class="block font-bold mb-2">公告日期</label>
                    <DatePicker v-model="selectedAnnouncement.announcementDate" :minDate="minDate" placeholder="公告日期不得低於今日" :showIcon="true" :showButtonBar="true" class="w-full p-2 border rounded" />
                </div>
                <div class="mb-4">
                    <label class="block font-bold mb-2">公告圖示(限10MB以下)</label>
                    <div class="flex row-auto">
                        <div class="w-1/2">
                            <img v-if="selectedAnnouncement.productImgUrl" :src="selectedAnnouncement.productImgUrl" alt="公告圖片" class="rounded aspect-video object-cover" />
                        </div>
                        <div class="w-1/2">
                            <div v-if="previewEventImageUrl">
                                <img :src="previewEventImageUrl" alt="預覽圖片" class="rounded aspect-video object-cover" />
                            </div>
                            <!-- 預覽圖片顯示區 -->
                            <input type="file" @change="onFileChange" accept="image/jpeg, image/png, image/webp" />
                        </div>
                    </div>
                </div>
                <div class="mb-4">
                    <label class="block font-bold mb-2">公告詳細描述</label>
                    <textarea v-model="selectedAnnouncement.content" placeholder="詳細描述" class="w-full p-2 border rounded"></textarea>
                </div>
            </div>
            <div class="flex justify-end mt-4">
                <Button label="取消" icon="pi pi-times" text @click="editDialog = false" />
                <Button label="保存公告" icon="pi pi-check" @click="saveAnnouncement" />
            </div>
        </Dialog>
        <!-- 制定產品折扣 -->
        <Dialog v-model:visible="discountProductsDialog" :style="{ width: '800px' }" header="制定折扣" :modal="true">
            <div>
                <h3 class="font-bold mb-2 text-lg">建立新的折扣活動</h3>
                <!-- 折扣起始日期 -->
                <div class="flex mb-10">
                    <div class="mr-3">
                        <label for="discountStartDate" class="block font-bold mb-3">起始日</label>
                        <input
                            type="date"
                            id="discountStartDate"
                            v-model="discountStartDate"
                            class="w-full px-3 py-2 border rounded-lg text-gray-200 bg-gray-800 border-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-blue-400 transition duration-200 ease-in-out"
                            :min="discountSetMinDate"
                        />
                    </div>
                    <!-- 折扣結束日期 -->
                    <div class="mr-5">
                        <label for="discountEndDate" class="block font-bold mb-3">結束日</label>
                        <input
                            type="date"
                            id="discountEndDate"
                            v-model="discountEndDate"
                            class="w-full px-3 py-2 border rounded-lg text-gray-200 bg-gray-800 border-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-blue-400 transition duration-200 ease-in-out"
                            :min="discountSetMinDate"
                        />
                    </div>
                    <!-- 折扣值 -->
                    <div class="mr-3">
                        <label for="discountValue" class="block font-bold mb-3">折扣</label>
                        <div class="relative inline-block">
                            <input
                                type="text"
                                v-model="discountValue"
                                @input="validateDiscountInput"
                                maxlength="3"
                                class="pl-6 pr-6 px-3 py-2 border rounded-lg text-gray-200 bg-gray-800 border-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-blue-400 transition duration-200 ease-in-out"
                                style="width: 5rem"
                            />
                            <span class="absolute left-1 top-1/2 transform -translate-y-1/2 text-gray-300">-</span>
                            <span class="absolute right-1 top-1/2 transform -translate-y-1/2 text-gray-300">%</span>
                        </div>
                    </div>
                </div>

                <!-- 已選擇的產品折扣活動 -->
                <div class="mt-5">
                    <h3 class="font-bold mb-2 text-lg">所選產品目前擁有的折扣活動</h3>

                    <div v-if="discountActivities.length === 0" class="text-gray-500">沒有任何折扣活動。</div>

                    <div v-for="(activity, index) in discountActivities" :key="index" class="flex justify-between items-center border-b py-2">
                        <div class="w-1/4">
                            <div class="mb-3">ID : {{ activity.productId }} <br /></div>
                            <div>遊戲名稱 : {{ activity.productName }} <br /></div>
                        </div>

                        <div class="mr-3">
                            <label :for="'discountStartDate' + index" class="block font-bold mb-3">起始日</label>
                            <input type="date" :id="'discountStartDate' + index" v-model="activity.discountStartDate" class="" :min="minDate" disabled style="background-color: #18181b" />
                        </div>
                        <div class="mr-5">
                            <label :for="'discountEndDate' + index" class="block font-bold mb-3">結束日</label>
                            <input type="date" :id="'discountEndDate' + index" v-model="activity.discountEndDate" class="" :min="minDate" disabled style="background-color: #18181b" />
                        </div>
                        <div class="mr-3">
                            <label for="discountValue" class="block font-bold mb-3">折扣</label>
                            <div class="">
                                <span>- </span>

                                <input id="'discountValue' + index" type="text" v-model="activity.discountValue" @input="validateDiscountInput" maxlength="3" style="width: 3rem; background-color: #18181b" disabled />

                                <span>%</span>
                            </div>
                        </div>

                        <Button label="刪除" icon="pi pi-times" class="p-button-danger" @click="confirmDeleteDisocuntActivity(index)" />
                    </div>
                </div>
            </div>
            <template #footer>
                <Button label="取消" icon="pi pi-times" text @click="discountProductsDialog = false" />
                <Button label="新增折扣活動" icon="pi pi-check" @click="discountSelectedProducts" />
            </template>
        </Dialog>

        <!-- 刪除折扣活動確認modal -->
        <Dialog v-model:visible="deleteDiscountDialog" :style="{ width: '450px' }" header="Confirm" :modal="true">
            <div class="flex items-center gap-4">
                <i class="pi pi-exclamation-triangle !text-3xl" />
                <span v-if="product">您確定要刪除此折扣活動嗎?</span>
            </div>

            <template #footer>
                <Button label="No" icon="pi pi-times" text @click="deleteDiscountDialog = false" />
                <Button label="Yes" icon="pi pi-check" @click="removeDiscountActivity" />
            </template>
        </Dialog>

        <!-- 上架產品modal -->
        <Dialog v-model:visible="activatedProductDialog" :style="{ width: '450px' }" header="Confirm" :modal="true">
            <div class="flex items-center gap-4">
                <i class="pi pi-exclamation-triangle !text-3xl" />
                <span v-if="product"
                    >您確定要上架 <b>{{ product.productName }}</b> 嗎?</span
                >
            </div>
            <template #footer>
                <Button label="No" icon="pi pi-times" text @click="activatedProductDialog = false" />
                <Button label="Yes" icon="pi pi-check" @click="activatedProduct" />
            </template>
        </Dialog>

        <Dialog v-model:visible="activatedProductsDialog" :style="{ width: '450px' }" header="Confirm" :modal="true">
            <div class="flex items-center gap-4">
                <i class="pi pi-exclamation-triangle !text-3xl" />
                <span v-if="product">您確定要上架被選取的產品嗎</span>
            </div>
            <template #footer>
                <Button label="No" icon="pi pi-times" text @click="activatedProductsDialog = false" />
                <Button label="Yes" icon="pi pi-check" text @click="activatedSelectedProducts" />
            </template>
        </Dialog>

        <!-- 下架產品modal -->
        <Dialog v-model:visible="deactivatedProductDialog" :style="{ width: '450px' }" header="Confirm" :modal="true">
            <div class="flex items-center gap-4">
                <i class="pi pi-exclamation-triangle !text-3xl" />
                <span v-if="product"
                    >您確定要下架 <b>{{ product.productName }}</b> 嗎?</span
                >
            </div>
            <template #footer>
                <Button label="No" icon="pi pi-times" text @click="deactivatedProductDialog = false" />
                <Button label="Yes" icon="pi pi-check" @click="deactivatedProduct" />
            </template>
        </Dialog>

        <Dialog v-model:visible="deactivatedProductsDialog" :style="{ width: '450px' }" header="Confirm" :modal="true">
            <div class="flex items-center gap-4">
                <i class="pi pi-exclamation-triangle !text-3xl" />
                <span v-if="product">您確定要下架被選取的產品嗎</span>
            </div>
            <template #footer>
                <Button label="No" icon="pi pi-times" text @click="deactivatedProductsDialog = false" />
                <Button label="Yes" icon="pi pi-check" text @click="deactivatedSelectedProducts" />
            </template>
        </Dialog>

        <!--Delete Modal -->
        <Dialog v-model:visible="deleteProductDialog" :style="{ width: '450px' }" header="Confirm" :modal="true">
            <div class="flex items-center gap-4">
                <i class="pi pi-exclamation-triangle !text-3xl" />
                <span v-if="product"
                    >您確定要刪除 <b>{{ product.productName }}</b> 嗎?</span
                >
            </div>
            <template #footer>
                <Button label="No" icon="pi pi-times" text @click="deleteProductDialog = false" />
                <Button label="Yes" icon="pi pi-check" @click="deleteProduct" />
            </template>
        </Dialog>

        <Dialog v-model:visible="deleteProductsDialog" :style="{ width: '450px' }" header="Confirm" :modal="true">
            <div class="flex items-center gap-4">
                <i class="pi pi-exclamation-triangle !text-3xl" />
                <span v-if="product">您確定要刪除被選取的產品嗎</span>
            </div>
            <template #footer>
                <Button label="No" icon="pi pi-times" text @click="deleteProductsDialog = false" />
                <Button label="Yes" icon="pi pi-check" text @click="deleteSelectedProducts" />
            </template>
        </Dialog>

        <Dialog v-model:visible="ProductsMainImageDialog" :style="{ width: '700px' }" header="遊戲封面圖片" :modal="true">
            <div class="flex flex-row">
                <div class="flex flex-col items-left gap-4 p-3 w-[50%]">
                    <span>目前封面圖片</span>
                    <div v-if="product.productMainImageUrl">
                        <img :src="product.productMainImageUrl" alt="Preview" class="mt-2 max-w-full h-auto" />
                    </div>
                </div>
                <div class="flex flex-col items-left gap-4 p-3 w-[50%]">
                    <span>上傳的圖片</span>
                    <div v-if="previewImage">
                        <img :src="previewImage" alt="Preview" class="mt-2 max-w-full h-auto" />
                    </div>
                    <span><input type="file" @change="onMainImgChange" accept="image/jpeg, image/png, image/webp" /></span>
                </div>
            </div>
            <template #footer>
                <Button label="No" icon="pi pi-times" text @click="closeMainImgDialog" />
                <Button label="上傳" icon="pi pi-check" text @click="saveMainImg" />
            </template>
        </Dialog>
    </div>
    <div v-if="loading" class="loading-indicator">
        <div class="fixed loading-title">正在上傳中，請稍候...</div>
        <div>
            <img src="https://res.cloudinary.com/dijn0xzac/image/upload/v1728299253/eofcjued0iellxzha0i9.gif" alt="" />
        </div>
    </div>
</template>

<style>
.progress-container {
    display: flex;
    flex-direction: column;
    align-items: flex-start;
}

.progress-bar-wrapper {
    width: 100%;
    height: 10px;
    display: flex;
    background-color: #f0f0f0;
    border-radius: 4px;
    overflow: hidden;
    position: relative;
}

.progress-bar {
    height: 100%;
}

.positive {
    background-color: #66c0f4;
}

.negative {
    background-color: #a34c25;
}

.progress-labels {
    margin-top: 5px;
    display: flex;
    justify-content: space-between;
    width: 100%;
}

.good-comment-label {
    color: #66c0f4;
    font-size: 10px;
}

.bad-comment-label {
    color: #a34c25;
    font-size: 10px;
}

.progress-bar-filler {
    width: 100%;
}

.p-datatable-tbody > tr > td {
    height: 64px;
}
</style>

<style>
.media-item-carousel {
    width: 100%;
    position: relative;
    margin: 10px;
}

.media-item-carousel img,
.media-item-carousel video {
    max-width: 200px;
    aspect-ratio: 16 / 9;
}

.media-item-carousel p {
    margin-top: 10px;
    word-break: break-word;
}

.media-item-carousel button {
    width: max-content;
    position: absolute;
    bottom: 0;
    right: 20px;
    background-color: #f44336;
    color: white;
    padding: 5px 10px;
    border: none;
    border-radius: 4px;
    cursor: pointer;
}

.media-item-carousel button:hover {
    background-color: #d32f2f;
}

.sortable-ghost {
    opacity: 0.4;
}
</style>

<style>
/* 遮罩層覆蓋整個頁面 */
.loading-indicator {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background-color: rgba(0, 0, 0, 0.5); /* 半透明背景 */
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 99999; /* 確保它覆蓋其他內容 */
}
.loading-title {
    font-size: 20px;
    font-weight: bolder;
    color: #80c;
    left: 46%;
    top: 64%;
}
</style>
