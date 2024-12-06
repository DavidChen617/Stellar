<script setup>
import { ref } from 'vue';

import AppMenuItem from './AppMenuItem.vue';

const model = ref([
    {
        label: 'Home',
        items: [{
                label: 'DashBoard',
                icon: 'pi pi-fw pi-chart-scatter',
                to: '/pages/charts',
            },]
    },
    {
        label: 'Pages',
        icon: 'pi pi-fw pi-briefcase',
        to: '/pages',
        items: [
            {
                label: '產品管理',
                icon: 'pi pi-shopping-bag',
                to: '/pages/AllProduct'
            },
            {
                label: '產品上架',
                icon: 'pi pi-prime',
                to: '/pages/ProductLaunch'
            },
            {
                label: '分類與標籤',
                icon: 'pi pi-slack',
                to: '/pages/Category&Tag'
            },
            // {
            //     label: 'TodoItems代辦事項',
            //     icon: 'pi pi-spin pi-spinner',
            //     to: '/pages/TodoItems'
            // },
            {
                label: '使用者管理',
                icon: 'pi pi-users',
                to: '/pages/UserList'
            },
            {
                label: '供應商管理',
                icon: 'pi pi-fw pi-id-card',
                to: '/pages/Publishers'
            },
            // {
            //     label: 'Landing',
            //     icon: 'pi pi-fw pi-globe',
            //     to: '/landing'
            // },
            // {
            //     label: '驗證',
            //     icon: 'pi pi-fw pi-user',
            //     items: [
            //         {
            //             label: '登入',
            //             icon: 'pi pi-fw pi-sign-in',
            //             to: '/auth/login'
            //         },
            //         // {
            //         //     label: 'Error',
            //         //     icon: 'pi pi-fw pi-times-circle',
            //         //     to: '/auth/error'
            //         // },
            //         // {
            //         //     label: 'Access Denied',
            //         //     icon: 'pi pi-fw pi-lock',
            //         //     to: '/auth/access'
            //         // }
            //     ]
            // },



    //         {
    //         label: '樣板Component',
    //         icon:"pi pi-fw pi-wrench",
    //         items: [
    //             {label: 'Dashboard', icon: 'pi pi-fw pi-home', to: '/'},
    //             { label: 'Form Layout', icon: 'pi pi-fw pi-id-card', to: '/uikit/formlayout' },
    //             { label: 'Input', icon: 'pi pi-fw pi-check-square', to: '/uikit/input' },
    //             { label: 'Button', icon: 'pi pi-fw pi-mobile', to: '/uikit/button', class: 'rotated-icon' },
    //             { label: 'Table', icon: 'pi pi-fw pi-table', to: '/uikit/table' },
    //             { label: 'List', icon: 'pi pi-fw pi-list', to: '/uikit/list' },
    //             { label: 'Tree', icon: 'pi pi-fw pi-share-alt', to: '/uikit/tree' },
    //             { label: 'Panel', icon: 'pi pi-fw pi-tablet', to: '/uikit/panel' },
    //             { label: 'Overlay', icon: 'pi pi-fw pi-clone', to: '/uikit/overlay' },
    //             { label: 'Media', icon: 'pi pi-fw pi-image', to: '/uikit/media' },
    //             { label: 'Menu', icon: 'pi pi-fw pi-bars', to: '/uikit/menu' },
    //             { label: 'Message', icon: 'pi pi-fw pi-comment', to: '/uikit/message' },
    //             { label: 'File', icon: 'pi pi-fw pi-file', to: '/uikit/file' },
    //             { label: 'Chart', icon: 'pi pi-fw pi-chart-bar', to: '/uikit/charts' },
    //             { label: 'Timeline', icon: 'pi pi-fw pi-calendar', to: '/uikit/timeline' },
    //             { label: 'Misc', icon: 'pi pi-fw pi-circle', to: '/uikit/misc' },
    //             { label: 'Not Found', icon: 'pi pi-fw pi-exclamation-circle', to: '/pages/notfound'},
    //             { label: 'Empty', icon: 'pi pi-fw pi-circle-off', to: '/pages/empty'},
    //             { label: 'Crud', icon: 'pi pi-fw pi-pencil', to: '/pages/crud' },
    //     ]
    // },
        ]
    },
    // {
    //     label: 'Hierarchy',
    //     items: [
    //         {
    //             label: 'Submenu 1',
    //             icon: 'pi pi-fw pi-bookmark',
    //             items: [
    //                 {
    //                     label: 'Submenu 1.1',
    //                     icon: 'pi pi-fw pi-bookmark',
    //                     items: [
    //                         { label: 'Submenu 1.1.1', icon: 'pi pi-fw pi-bookmark' },
    //                         { label: 'Submenu 1.1.2', icon: 'pi pi-fw pi-bookmark' },
    //                         { label: 'Submenu 1.1.3', icon: 'pi pi-fw pi-bookmark' }
    //                     ]
    //                 },
    //                 {
    //                     label: 'Submenu 1.2',
    //                     icon: 'pi pi-fw pi-bookmark',
    //                     items: [{ label: 'Submenu 1.2.1', icon: 'pi pi-fw pi-bookmark' }]
    //                 }
    //             ]
    //         },
    //         {
    //             label: 'Submenu 2',
    //             icon: 'pi pi-fw pi-bookmark',
    //             items: [
    //                 {
    //                     label: 'Submenu 2.1',
    //                     icon: 'pi pi-fw pi-bookmark',
    //                     items: [
    //                         { label: 'Submenu 2.1.1', icon: 'pi pi-fw pi-bookmark' },
    //                         { label: 'Submenu 2.1.2', icon: 'pi pi-fw pi-bookmark' }
    //                     ]
    //                 },
    //                 {
    //                     label: 'Submenu 2.2',
    //                     icon: 'pi pi-fw pi-bookmark',
    //                     items: [{ label: 'Submenu 2.2.1', icon: 'pi pi-fw pi-bookmark' }]
    //                 }
    //             ]
    //         }
    //     ]
    // },
    // {
    //     label: 'Get Started',
    //     items: [
    //         {
    //             label: 'Documentation',
    //             icon: 'pi pi-fw pi-book',
    //             to: '/documentation'
    //         },
    //         {
    //             label: 'View Source',
    //             icon: 'pi pi-fw pi-github',
    //             url: 'https://github.com/primefaces/sakai-vue',
    //             target: '_blank'
    //         }
    //     ]
    // }
]);




</script>

<template>
    <ul class="layout-menu">
        <template v-for="(item, i) in model" :key="item">
            <app-menu-item v-if="!item.separator" :item="item" :index="i"></app-menu-item>
            <li v-if="item.separator" class="menu-separator"></li>
        </template>
    </ul>
    
</template>

<style lang="scss" scoped></style>
