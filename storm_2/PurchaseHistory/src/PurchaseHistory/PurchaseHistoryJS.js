var { createApp, ref, reactive, onMounted, computed } = Vue;
// const showOrderDetail = ref(true);  
createApp({
    setup() {
        const account = ref('');
        const purchaseHistoryList = ref([]);
        const loading = ref(false);
        const order = ref({
            histroyDetailProducts: [] // 初始化為一個空陣列
        });
        // const orderId = ref('');
        const showOrderDetail = ref(false);  // 控制顯示的模板
        const fetchPurchaseHistory = async () => {
            try {
                const response = await axios.get('https://localhost:7196/api/MemberApi/PurchaseHistory');
                // loading=true;
                account.value = response.data.userAccount; // 更新 account
                purchaseHistoryList.value.push(...response.data.purchaseHistoryList);
                console.log(purchaseHistoryList)
                // purchaseHistoryList.forEach(x => console.log(x.productList))
                console.log(purchaseHistoryList.value)
            } catch (error) {
                console.error('Error fetching purchase history:', error);

            } finally {
                loading.value = false; // 結束加載
            }
        };
        const viewOrderDetail = async (id) => {
            console.log(id)
            loading.value = true; // 設置加載狀態為 true
            showOrderDetail.value = true;
            // orderId.value = id; // 設置所需的 orderId
            // showOrderDetail.value = true; // 切換至訂單詳情模板
            console.log(showOrderDetail.value)
            try {
                const response = await axios.get(`https://localhost:7196/api/Memberapi/OrderDetail/${id}`);
                order.value = await response.data;
                console.log(order.value.histroyDetailProducts) //products
                // console.log(order.value.orderId) 
                // console.log(order.value.paymentType)
                // console.log(order.value.productDiscount) 
                // console.log(order.value.purchaseDate) 
                // console.log(order.value.subtotal) 
                // console.log(order.value.totalPrice) 
        
            } catch (error) {
                console.error('無法獲取訂單詳情:', error);
            } finally {
                loading.value = false; // 結束加載
            }
        };
        const backToHistory = () => {
            showOrderDetail.value = false; // 返回到購買紀錄頁面
            console.log( showOrderDetail.value )
        };

        const searchQuery = ref(''); // 正確的初始化方式

        // 計算過濾後的訂單
        const filteredGames = computed(() => {
            const query = searchQuery.value.trim().toLowerCase();

            if (!query) {
                return purchaseHistoryList.value; // 當沒有搜尋時返回所有訂單
            }

            return purchaseHistoryList.value.filter(order =>
                order.productList.some(product =>
                    product.productName.trim().toLowerCase().includes(query) // 修正：使用 trim()
                )
            );
        });
        const formatDate = (dateString) => {
            const options = { year: 'numeric', month: '2-digit', day: '2-digit' };
            return new Date(dateString).toLocaleDateString('zh-TW', options);
        };


        onMounted(() => {
            fetchPurchaseHistory();
        });

        return {     
            order,
            loading,
            account,
            purchaseHistoryList,
            searchQuery,
            filteredGames,
            showOrderDetail,
            backToHistory,
            viewOrderDetail,
            formatDate
        };
    }
}).mount('#PurchaseHistory');


// ===========================================
// createApp({
//     data() {
//         const showOrderDetail = ref(true);  // 控制顯示的
//         return {
//             showOrderDetail,
//             order: {
//                 orderId: null,
//                 paymentType: null,
//                 purchaseDate: '',
//                 histroyDetailProducts: [],
//                 productDiscount: 0,
//                 subtotal: 0,
//                 totalPrice: 0
//             },
//         };
//     },
//     methods: {
//         async fetchPurchaseHistory() {
//             try {
//                 const response = await axios.get(`localhost:7196/api/Member/OrderDetail${orderId}`);
//                 this.order = response.data;
//             } catch (error) {

//             }
//         },

//         formatDate(dateString) {
//             const options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' };
//             return new Date(dateString).toLocaleDateString('zh-TW', options);
//         }
//     },
//     mounted() {
//         this.fetchPurchaseHistory();
//     }
// }).mount('#orderDetail');









// var { createApp, ref, onMounted } = Vue;

// createApp({
//     setup() {
//         const loading = ref(false);
//         const purchaseHistoryList = ref([]);

//         const fetchPurchaseHistory = async () => {
//             loading.value = true;
//             try {
//                 const response = await axios.get('https://localhost:7196/api/MemberApi/PurchaseHistory');
//                 purchaseHistoryList.value = response.data.purchaseHistoryList; // 取出購買紀錄
//             } catch (error) {
//                 console.error('無法獲取購買紀錄:', error);
//             } finally {
//                 loading.value = false; // 結束加載
//             }
//         };

//         onMounted(() => {
//             fetchPurchaseHistory(); // 組件掛載時請求訂單資料
//         });

//         return {
//             loading,
//             purchaseHistoryList
//         };
//     }
// }).mount('#purchaseHistory');