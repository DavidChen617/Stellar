<script setup>
import { useLayout } from '@/layout/composables/layout';
import { useRouter } from 'vue-router'; // 引入 useRouter 以進行路由導航

import Cookies from 'js-cookie';

import AppConfigurator from './AppConfigurator.vue';
import { ref } from 'vue';
const { onMenuToggle, toggleDarkMode, isDarkTheme } = useLayout();

const displayConfirmation = ref(false);
const router = useRouter(); // 使用 useRouter 來獲取路由實例

function openConfirmation() {
    displayConfirmation.value = true;
}

function closeConfirmation() {
    displayConfirmation.value = false;
}

// 登出功能
const logout = () => {
    // 清除 Cookies 中的 JWT
    Cookies.remove('token');

    // 重定向到登入頁面
    router.push('/auth/login');
    // // 清除 JWT
    // localStorage.removeItem('token');
    // // 重定向到登入頁面或其他頁面
    // router.push('/auth/login');
};
</script>

<template>
    <div class="layout-topbar">
        <div class="layout-topbar-logo-container">
            <button class="layout-menu-button layout-topbar-action" @click="onMenuToggle">
                <i class="pi pi-bars"></i>
            </button>
            <router-link to="/" class="layout-topbar-logo">
                <img src="https://res.cloudinary.com/dijn0xzac/image/upload/v1726576767/icon_stellar_wkohxy.png" style="width: 40px" />
                <span>Stellar</span>
            </router-link>
        </div>

        <div class="layout-topbar-actions">
            <div class="layout-config-menu">
                <button type="button" class="layout-topbar-action" @click="toggleDarkMode">
                    <i :class="['pi', { 'pi-moon': isDarkTheme, 'pi-sun': !isDarkTheme }]"></i>
                </button>
                <div class="relative">
                    <button
                        v-styleclass="{ selector: '@next', enterFromClass: 'hidden', enterActiveClass: 'animate-scalein', leaveToClass: 'hidden', leaveActiveClass: 'animate-fadeout', hideOnOutsideClick: true }"
                        type="button"
                        class="layout-topbar-action layout-topbar-action-highlight"
                    >
                        <i class="pi pi-palette"></i>
                    </button>
                    <AppConfigurator />
                </div>
            </div>

            <button
                class="layout-topbar-menu-button layout-topbar-action"
                v-styleclass="{ selector: '@next', enterFromClass: 'hidden', enterActiveClass: 'animate-scalein', leaveToClass: 'hidden', leaveActiveClass: 'animate-fadeout', hideOnOutsideClick: true }"
            >
                <i class="pi pi-ellipsis-v"></i>
            </button>

            <div class="layout-topbar-menu hidden lg:block">
                <div class="layout-topbar-menu-content">
                    <!-- <button type="button" class="layout-topbar-action">
                        <i class="pi pi-calendar"></i>
                        <span>Calendar</span>
                    </button>
                    <button type="button" class="layout-topbar-action">
                        <i class="pi pi-inbox"></i>
                        <span>Messages</span>
                    </button>
                    <button type="button" class="layout-topbar-action">
                        <i class="pi pi-user"></i>
                        <span>Profile</span>
                    </button> -->
                    <div class="content-evenly layout-topbar-action pt-[4px]">
                        <button label="Delete" icon="pi pi-trash" severity="danger" style="width: auto" @click="openConfirmation">
                            <i class="pi pi-sign-out"></i>
                            <!-- <span>Sign-out</span>  -->
                        </button>
                        <Dialog header="Stellar" v-model:visible="displayConfirmation" :style="{ width: '350px' }" :modal="true">
                            <div class="flex items-center justify-center">
                                <span class="text-[20px]">確定要登出嗎 ?</span>
                            </div>
                            <template #footer>
                                <Button label="No" icon="pi pi-times" @click="closeConfirmation" text severity="secondary" />
                                <Button label="Yes" icon="pi pi-check" @click="logout" severity="danger" outlined autofocus />
                            </template>
                        </Dialog>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
