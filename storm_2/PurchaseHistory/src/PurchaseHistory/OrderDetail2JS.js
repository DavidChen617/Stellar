
// const {onMounted } = Vue;
// const showOrderDetail = ref(true);

// const orderId = this.$route.params.id;
createApp({
    data() {
        return {
            showOrderDetail,
            order: {
                orderId: null,
                paymentType: null,
                purchaseDate: '',
                histroyDetailProducts: [],
                productDiscount: 0,
                subtotal: 0,
                totalPrice: 0
            },
        };
    },
    methods: {
        async fetchPurchaseHistory() {
            try {
                const response = await axios.get(`localhost:7196/api/Member/OrderDetail${orderId}`);
                this.order = response.data;
            } catch (error) {

            }
        },
     
    formatDate(dateString) {
        const options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' };
        return new Date(dateString).toLocaleDateString('zh-TW', options);
    }
},
    mounted() {
    this.fetchPurchaseHistory();
}
}).mount('#orderDetail');





// var { createApp, ref, onMounted } = Vue;

// createApp({
//     setup() {
//         const order = ref({
//             orderId: null,
//             paymentType: null,
//             purchaseDate: '',
//             histroyDetailProducts: [],
//             productDiscount: 0,
//             subtotal: 0,
//             totalPrice: 0
//         });

//         const orderId = new URLSearchParams(window.location.search).get('id'); // 獲取 URL 查詢參數中的 orderId
// console.log(orderId)
//         const fetchPurchaseHistory = async () => {
//             try {
//                 const response = await axios.get(`https://localhost:7196/api/Member/OrderDetail/${orderId}`); // 注意添加協議
//                 order.value = response.data; // 設定獲得的訂單詳細資料
//             } catch (error) {
//                 console.error('無法獲取訂單詳細資訊:', error);
//             }
//         };
       
//         const formatDate = (dateString) => {
//             const options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' };
//             return new Date(dateString).toLocaleString('zh-TW', options);
//         };

//         onMounted(() => {
//             fetchPurchaseHistory(); // 組件掛載時請求訂單資料
//         });

//         // 返回 setup 中的響應式變量和方法
//         return { order, formatDate };
//     }
// }).mount('#orderDetail');




// var { createApp, ref, onMounted } = Vue;

// createApp({
//     setup() {
//         const order = ref(null);
//         const loading = ref(false);
        
//         // 從 URL 獲取查詢參數
//         const getOrderIdFromUrl = () => {
//             const urlParams = new URLSearchParams(window.location.search);
//             return urlParams.get('id');
//         };

//         const fetchOrderDetails = async () => {
//             loading.value = true; // 開始加載
//             const orderId = getOrderIdFromUrl(); // 獲取訂單 ID

//             try {
//                 const response = await axios.get(`https://localhost:7196/api/Member/OrderDetail/${orderId}`);
//                 order.value = response.data; // 設定獲取的訂單詳情資料
//             } catch (error) {
//                 console.error('無法獲取訂單詳情:', error);
//             } finally {
//                 loading.value = false; // 結束加載
//             }
//         };

//         const formatDate = (dateString) => {
//             const options = { year: 'numeric', month: '2-digit', day: '2-digit' };
//             return new Date(dateString).toLocaleDateString('zh-TW', options);
//         };

//         onMounted(() => {
//             fetchOrderDetails(); // 當組件掛載時請求訂單資料
//         });

//         return {
//             loading,
//             order,
//             formatDate
//         };
//     }
// }).mount('#orderDetail');