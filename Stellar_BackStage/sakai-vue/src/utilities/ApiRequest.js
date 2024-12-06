import axios from 'axios';
import { getToken } from '@/utilities/TokenHelper';

const LOGIN_PAGE_URL = '/auth/login';
const FORBIDDEN_PAGE_URL = '/auth/access';
const handle401Unauthorized = () => {
    window.location.href = LOGIN_PAGE_URL;
};
const handle403Forbidden = () => {
    window.location.href = FORBIDDEN_PAGE_URL;
};

const useRequest = function () {
    // Note: 請根據後端API位置調整(.env)
    const API_BASE_URL = import.meta.env.VITE_API_HOST;
    const request = axios.create({ baseURL: API_BASE_URL });

    const beforeRequest = (config) => {
        // 發 request 前處理

        // 如果有 JWT Token 就帶
        const token = getToken();
        token && (config.headers['Authorization'] = `Bearer ${token}`);

        return config;
    };

    // 請求攔截器
    request.interceptors.request.use(beforeRequest);

    const responseSuccess = (response) => {
        // 2XX
        // NOTE: 請根據後端API接口做調整

        // console.log(response)
        return response.data;
    };

    const responseFail = (err) => {
        try {
            // !2XX
            const { response } = err;
            const { status } = response;

            // NOTE: 統一處理失敗行為
            switch (status) {
                case 401:
                    handle401Unauthorized();
                    break;
                case 403:
                    handle403Forbidden();
                    break;
            }

            return Promise.reject(response);
        } catch (error) {
            return Promise.reject(error);
        }
    };

    // 回應攔截器
    request.interceptors.response.use(responseSuccess, responseFail);

    return {
        httpGet: request.get,
        httpPost: request.post,
        httpPut: request.put,
        httpDelete: request.delete,
        postForm: request.postForm
    };
};

export default useRequest();