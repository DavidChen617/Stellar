<script setup>
import FloatingConfigurator from '@/components/FloatingConfigurator.vue';
import { ref } from 'vue';
import { loginService } from '@/service/AuthLoginService';
import { useRouter, useRoute } from 'vue-router'; // 引入 useRouter 以進行路由導航
import Cookies from 'js-cookie'; // 引入 js-cookie
import Checkbox from 'primevue/checkbox';
const userName = ref('');
const password = ref('');
const checked = ref(false);
const errorMessage = ref('');
const router = useRouter(); // 使用 useRouter 來獲取路由實例
const route = useRoute();
const submitted = ref(false);
const login = async () => {
    submitted.value = true;
    try {
        const response = await loginService.login({
            userName: userName.value,
            password: password.value
        });

        if (!response.isSuccess) {
            // 登入失敗，導向錯誤頁面
            errorMessage.value = response.errMsg || '登入失敗，請檢查使用者名稱和密碼。';
            router.push('/auth/error'); // 導向錯誤頁面
        } else {
            const options = {
                expires: checked.value ? 1 : undefined, // 如果選中 "記住我"，設置 Cookie 過期時間為 1 天
                secure: true,
                sameSite: 'Strict'
            };
            Cookies.set('token', response.body.token, options);
            errorMessage.value = '';
            const redirect = route.query.redirect || '/pages/charts';
            router.push(redirect);
        }
    } catch (err) {
        console.log(err);
    }
};
const toggleCheckbox = () => {
    checked.value = !checked.value;
};
</script>

<template>
    <FloatingConfigurator />
    <div class="bg-surface-50 dark:bg-surface-950 flex items-center justify-center min-h-screen min-w-[100vw] overflow-hidden">
        <div class="flex flex-col items-center justify-center input-container">
            <div style="border-radius: 56px; padding: 0.3rem; background: linear-gradient(180deg, var(--primary-color) 10%, rgba(33, 150, 243, 0) 30%)">
                <div class="w-full bg-surface-0 dark:bg-surface-900 py-16 px-8 sm:px-20" style="border-radius: 53px">
                    <div class="text-center mb-8">
                        <div style="text-align: -webkit-center">
                            <img src="../../../imgs/logo_steam.png" alt="" class="w-[100px]" />
                        </div>

                        <div class="text-surface-900 dark:text-surface-0 text-3xl font-medium my-8 title-2">Welcome to Stellar!</div>
                        <span class="text-muted-color font-medium">Sign in to continue</span>
                    </div>

                    <section class="bg-stars">
                        <span class="star"></span>
                        <span class="star"></span>
                        <span class="star"></span>
                        <span class="star"></span>
                    </section>

                    <div>
                        <div class="input-dist">
                            <small v-if="submitted && !userName" class="text-red-500">Account is required</small>
                            <InputText class="w-full input-is" id="account" placeholder="Account" v-model="userName" required="" autofocus :invalid="submitted && !userName" fluid />
                        </div>
                        <div class="input-dist">
                            <small v-if="submitted && !password" class="text-red-500">Password is required</small>
                            <InputText class="input-is" v-model="password" placeholder="Password" type="password" required="" :invalid="submitted && !password" fluid />
                        </div>

                        <div class="flex items-center justify-between mt-2 gap-8">
                            <div class="flex items-center">
                                <Checkbox v-model="checked" id="rememberme" binary class="mr-2" label="Remember me" />
                                <label for="rememberme" @click="toggleCheckbox">Remember me</label>
                            </div>
                            <!-- <span class="font-medium no-underline ml-2 text-right cursor-pointer text-primary">Forgot password?</span> -->
                        </div>
                        <a @click="login" class="w-full login-btn text-center cursor-pointer" as="router-link" to="/">
                            <span></span>
                            <span></span>
                            <span></span>
                            <span></span>
                            Sign In
                        </a>

                        <div v-if="errorMessage" class="text-red-500 mt-4 text-center">{{ errorMessage }}</div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.pi-eye {
    transform: scale(1.6);
    margin-right: 1rem;
}

.pi-eye-slash {
    transform: scale(1.6);
    margin-right: 1rem;
}
</style>

<style>
.input-container {
    filter: drop-shadow(46px 36px 24px #4090b5) drop-shadow(-55px -40px 25px #9e30a9);
    animation: blinkShadowsFilter 8s ease-in infinite;
}
.input-dist {
    padding-block: 1.2em;
}

.input-is {
    color: hsl(207, 50%, 35%);;
    font-size: 0.9rem;
    background-color: transparent;
    width: 100%;
    box-sizing: border-box;
    padding-block: 0.7em;
    border: none;
    transition: all 1s ease-in-out;
    border-bottom: 1px solid hsl(221, 26%, 43%);
}

.input-is:hover {
    transition: all 1s ease-in-out;
    background: linear-gradient(90deg, transparent 0%, rgba(102, 224, 255, 0.2) 27%, rgba(102, 224, 255, 0.2) 63%, transparent 100%);
}

.input-is:focus {
    outline: none;
    border-bottom: 1px solid hsl(192, 100%, 100%);
    color: hsl(207, 50%, 35%);
    background: linear-gradient(90deg, transparent 0%, rgba(102, 224, 255, 0.2) 27%, rgba(102, 224, 255, 0.2) 63%, transparent 100%);
}

.input-is::-moz-placeholder {
    color: hsla(192, 100%, 88%, 0.806);
}

.input-is::placeholder {
    color: hsla(192, 100%, 88%, 0.806);
}

@keyframes backglitch {
    0% {
        box-shadow: inset 0px 20px 20px 30px #212121;
    }

    50% {
        box-shadow: inset 0px -20px 20px 30px hsl(297, 42%, 10%);
    }

    to {
        box-shadow: inset 0px 20px 20px 30px #212121;
    }
}

@keyframes rotate {
    0% {
        transform: rotate(0deg) translate(-50%, 20%);
    }

    50% {
        transform: rotate(180deg) translate(40%, 10%);
    }

    to {
        transform: rotate(360deg) translate(-50%, 20%);
    }
}

@keyframes blinkShadowsFilter {
    0% {
        filter: drop-shadow(46px 36px 28px rgba(64, 144, 181, 0.3411764706)) drop-shadow(-55px -40px 28px #9e30a9);
    }

    25% {
        filter: drop-shadow(46px -36px 24px rgba(64, 144, 181, 0.8980392157)) drop-shadow(-55px 40px 24px #9e30a9);
    }

    50% {
        filter: drop-shadow(46px 36px 30px rgba(64, 144, 181, 0.8980392157)) drop-shadow(-55px 40px 30px rgba(159, 48, 169, 0.2941176471));
    }

    75% {
        filter: drop-shadow(20px -18px 25px rgba(64, 144, 181, 0.8980392157)) drop-shadow(-20px 20px 25px rgba(159, 48, 169, 0.2941176471));
    }

    to {
        filter: drop-shadow(46px 36px 28px rgba(64, 144, 181, 0.3411764706)) drop-shadow(-55px -40px 28px #9e30a9);
    }
}
</style>

<style>
.title-2 {
    display: block;
    font-size: 2.1rem;
    font-weight: 800;
    font-family: Arial, Helvetica, sans-serif;
    text-align: center;
    -webkit-text-stroke: #fff 0.1rem;
    letter-spacing: 0.2rem;
    color: #000;
    position: relative;
    text-shadow: 0px 0px 30px #cecece;
}

.title-2 span::before,
.title-2 span::after {
    content: '—';
}

.bg-stars {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: -2;
    background-size: cover;
    animation: animateBg 50s linear infinite;
}
.star {
    position: absolute;
    top: 50%;
    left: 50%;
    width: 4px;
    height: 4px;
    background: #fff;
    border-radius: 50%;
    box-shadow:
        0 0 0 4px rgba(255, 255, 255, 0.1),
        0 0 0 8px rgba(255, 255, 255, 0.1),
        0 0 20px rgba(255, 255, 255, 0.1);
    animation: animate 3s linear infinite;
}

.star::before {
    content: '';
    position: absolute;
    top: 50%;
    transform: translateY(-50%);
    width: 300px;
    height: 1px;
    background: linear-gradient(90deg, #fff, transparent);
}

@keyframes animate {
    0% {
        transform: rotate(315deg) translateX(0);
        opacity: 1;
    }

    70% {
        opacity: 1;
    }

    100% {
        transform: rotate(315deg) translateX(-1000px);
        opacity: 0;
    }
}

.star:nth-child(1) {
    top: 0;
    right: 0;
    left: initial;
    animation-delay: 0s;
    animation-duration: 1s;
}

.star:nth-child(2) {
    top: 0;
    right: 100px;
    left: initial;
    animation-delay: 0.2s;
    animation-duration: 3s;
}

.star:nth-child(3) {
    top: 0;
    right: 220px;
    left: initial;
    animation-delay: 2.75s;
    animation-duration: 2.75s;
}

.star:nth-child(4) {
    top: 0;
    right: -220px;
    left: initial;
    animation-delay: 1.6s;
    animation-duration: 1.6s;
}
</style>

<style>
.login-btn {
    position: relative;
    display: inline-block;
    padding: 10px 20px;
    font-weight: bold;
    color: hsl(207, 50%, 35%);
    font-size: 16px;
    text-decoration: none;
    text-transform: uppercase;
    overflow: hidden;
    transition: 0.5s;
    margin-top: 40px;
    letter-spacing: 3px;
}

.login-btn:hover {
    background: #fff;
    color: #272727;
    border-radius: 5px;
}

.login-btn span {
    position: absolute;
    display: block;
}

.login-btn span:nth-child(1) {
    top: 0;
    left: -100%;
    width: 100%;
    height: 2px;
    background: linear-gradient(90deg, transparent, hsl(207, 50%, 35%));
    animation: btn-anim1 1.5s linear infinite;
}

@keyframes btn-anim1 {
    0% {
        left: -100%;
    }

    50%,
    100% {
        left: 100%;
    }
}

.login-btn span:nth-child(2) {
    top: -100%;
    right: 0;
    width: 2px;
    height: 100%;
    background: linear-gradient(180deg, transparent, hsl(207, 50%, 35%));
    animation: btn-anim2 1.5s linear infinite;
    animation-delay: 0.375s;
}

@keyframes btn-anim2 {
    0% {
        top: -100%;
    }

    50%,
    100% {
        top: 100%;
    }
}

.login-btn span:nth-child(3) {
    bottom: 0;
    right: -100%;
    width: 100%;
    height: 2px;
    background: linear-gradient(270deg, transparent, hsl(207, 50%, 35%));
    animation: btn-anim3 1.5s linear infinite;
    animation-delay: 0.75s;
}

@keyframes btn-anim3 {
    0% {
        right: -100%;
    }

    50%,
    100% {
        right: 100%;
    }
}

.login-btn span:nth-child(4) {
    bottom: -100%;
    left: 0;
    width: 2px;
    height: 100%;
    background: linear-gradient(360deg, transparent, hsl(207, 50%, 35%));
    animation: btn-anim4 1.5s linear infinite;
    animation-delay: 1.125s;
}

@keyframes btn-anim4 {
    0% {
        bottom: -100%;
    }

    50%,
    100% {
        bottom: 100%;
    }
}
</style>
