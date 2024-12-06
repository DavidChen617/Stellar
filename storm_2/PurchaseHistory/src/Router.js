const router = new VueRouter({
    routes: [
        {
            path: '/orders',
            name: 'orderHistory',
          
        },
        {
            path: '/Orderdetail/:id', 
            name: 'orderDetail',
          
        },
    ]
});
