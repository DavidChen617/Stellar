import AppLayout from '@/layout/AppLayout.vue';
import { createRouter, createWebHistory } from 'vue-router';
// import jwt_decode from 'jwt-decode'; // 使用命名導入 jwt_decode
import { jwtDecode } from "jwt-decode";

import Cookies from 'js-cookie';
const router = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: '/',
            redirect: '/auth/login'
        },
        {
            path: '/',
            component: AppLayout,
            meta: { requiresAuth: true }, 
            children: [
                {
                    path: '/dashboard',
                    name: 'dashboard',
                    component: () => import('@/views/Dashboard.vue')
                },
                {
                    path: '/uikit/formlayout',
                    name: 'formlayout',
                    component: () => import('@/views/uikit/FormLayout.vue')
                },
                {
                    path: '/uikit/input',
                    name: 'input',
                    component: () => import('@/views/uikit/InputDoc.vue')
                },
                {
                    path: '/uikit/button',
                    name: 'button',
                    component: () => import('@/views/uikit/ButtonDoc.vue')
                },
                {
                    path: '/uikit/table',
                    name: 'table',
                    component: () => import('@/views/uikit/TableDoc.vue')
                },
                {
                    path: '/uikit/list',
                    name: 'list',
                    component: () => import('@/views/uikit/ListDoc.vue')
                },
                {
                    path: '/uikit/tree',
                    name: 'tree',
                    component: () => import('@/views/uikit/TreeDoc.vue')
                },
                {
                    path: '/uikit/panel',
                    name: 'panel',
                    component: () => import('@/views/uikit/PanelsDoc.vue')
                },

                {
                    path: '/uikit/overlay',
                    name: 'overlay',
                    component: () => import('@/views/uikit/OverlayDoc.vue')
                },
                {
                    path: '/uikit/media',
                    name: 'media',
                    component: () => import('@/views/uikit/MediaDoc.vue')
                },
                {
                    path: '/uikit/message',
                    name: 'message',
                    component: () => import('@/views/uikit/MessagesDoc.vue')
                },
                {
                    path: '/uikit/file',
                    name: 'file',
                    component: () => import('@/views/uikit/FileDoc.vue')
                },
                {
                    path: '/uikit/menu',
                    name: 'menu',
                    component: () => import('@/views/uikit/MenuDoc.vue')
                },
                {
                    path: '/uikit/charts',
                    name: 'charts',
                    component: () => import('@/views/uikit/ChartDoc.vue')
                },
                {
                    path: '/uikit/misc',
                    name: 'misc',
                    component: () => import('@/views/uikit/MiscDoc.vue')
                },
                {
                    path: '/uikit/timeline',
                    name: 'timeline',
                    component: () => import('@/views/uikit/TimelineDoc.vue')
                },
                {
                    path: '/pages/empty',
                    name: 'empty',
                    component: () => import('@/views/pages/Empty.vue')
                },
                {
                    path: '/pages/crud',
                    name: 'crud',
                    component: () => import('@/views/pages/Crud.vue')
                },
                {
                    path: '/documentation',
                    name: 'documentation',
                    component: () => import('@/views/pages/Documentation.vue')
                },
                {
                    path: '/pages/todoItems',
                    name: 'todoItems',
                    component: () => import('@/views/pages/TodoItems.vue')
                },
                {
                    path: '/pages/northWind-shipper',
                    name: 'northWind-shipper',
                    component: () => import('@/views/pages/NorthWindShippers.vue')
                },
                {
                    path: '/pages/ProductLaunch',
                    name: 'ProductLaunch',
                    component: () => import('@/views/pages/ProductLaunch.vue')
                },
                {
                    path: '/pages/AllProduct',
                    name: 'AllProduct',
                    component: () => import('@/views/pages/AllProduct.vue')
                },
                {
                    path: '/pages/charts',
                    name: 'Charts',
                    component: () => import('@/views/pages/Charts.vue')
                },
                {
                    path: '/pages/userlist',
                    name: 'userlist',
                    component: () => import('@/views/pages/UserList.vue')
                },
                {
                    path: '/pages/publishers',
                    name: 'publishers',
                    component: () => import('@/views/pages/Publishers.vue')
                },
                 {
                    path: '/pages/category&tag',
                    name: 'category&tag',
                    component: () => import('@/views/pages/Category&Tag.vue')
                },

            ]
        },

        {
            path: '/landing',
            name: 'landing',
            component: () => import('@/views/pages/Landing.vue')
        },
        {
            path: '/pages/notfound',
            name: 'notfound',
            component: () => import('@/views/pages/NotFound.vue')
        },

        {
            path: '/auth/login',
            name: 'login',
            component: () => import('@/views/pages/auth/Login.vue')
        },
        {
            path: '/auth/access',
            name: 'accessDenied',
            component: () => import('@/views/pages/auth/Access.vue')
        },
        {
            path: '/auth/error',
            name: 'error',
            component: () => import('@/views/pages/auth/Error.vue')
        }
    ]
});

// 在全局前置守衛中進行 JWT 驗證
router.beforeEach((to, from, next) => {
    const requiresAuth = to.matched.some(record => record.meta.requiresAuth);
    const token = Cookies.get('token');
  
    if (requiresAuth) {
      if (token) {
        try {
          const decoded = jwtDecode(token);
          const currentTime = Date.now() / 1000;
  
          if (decoded.exp < currentTime) {
            // Token 已過期
            Cookies.remove('token');
            next({ name: 'login', query: { redirect: to.fullPath } });
          } else {
            // Token 有效
            next();
          }
        } catch (error) {
          console.error('無效的 token:', error);
          Cookies.remove('token');
          next({ name: 'login', query: { redirect: to.fullPath } });
        }
      } else {
        // 沒有 token，重定向到登錄頁面
        next({ name: 'login', query: { redirect: to.fullPath } });
      }
    } else {
      // 不需要身份驗證的路由
      next();
    }
  });

export default router;
