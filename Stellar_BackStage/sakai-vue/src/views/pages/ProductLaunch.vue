<script setup>
import { ref, onMounted, computed, reactive, nextTick, markRaw } from 'vue';
import DatePicker from 'primevue/datepicker';
import Sortable from 'sortablejs';
import Dialog from 'primevue/dialog';
import InputText from 'primevue/inputtext';
import { ProductLaunchService } from '@/service/ProductLaunchService';
import { PublisherService } from '@/service/PublisherService';
import Poor from '/src/data/Poor.vue';
import Average from '/src/data/Average.vue';
import Excellent from '/src/data/Excellent.vue';
import Super from '/src/data/Super.vue';
import { useToast } from 'primevue/usetoast';
import router from '@/router';
// 資料初始化

const gameName = ref('');
const selectedCategory = ref(null);
const categories = ref([]);
const selectedTags = ref(null);
const tags = ref([]);
const originalTags = ref([]);
const selectedpublisher = ref(null);
const publishers = ref([]);
const imagePreview = ref('');
const description = ref('');
const loading = ref(false); // 定義 loading 狀態
const submitted = ref(false);

onMounted(() => {
    Promise.all([ProductLaunchService.getCategories(), ProductLaunchService.getTags(), PublisherService.getPublishers()]).then(([categoriesData, tagsData, publisherData]) => {
        categories.value = categoriesData.map((category) => category.categoryId + ' ' + category.categoryName);
        originalTags.value = tagsData.map((tag) => ({
            id: tag.tagId,
            name: tag.tagName
        }));
        tags.value = [...originalTags.value];
        publishers.value = publisherData.filter((publisher) => publisher.status === '活動中').map((publisher) => publisher.publisherId + ' ' + publisher.publisherName);
    });
    nextTick(() => {
        const previewContainer = document.getElementById('media-preview');
        if (previewContainer) {
            Sortable.create(previewContainer, {
                animation: 150,
                ghostClass: 'sortable-ghost',
                handle: '.media-item', // 指定拖動的區域
                onEnd: updateMediaOrder // 拖動結束後更新順序
            });
        }
    });
});

const launchDate = ref(null);
// 發行日期不得低於今日
const minDate = computed(() => {
    return new Date(); // 返回當前日期作為最小日期
});
const sevenDate = computed(() => {
    const today = new Date();
    today.setDate(today.getDate() + 7);
    return today;
});
const toast = useToast();
const gamePrice = ref(0);
const mainImageFile = ref(null);
// 圖片預覽功能
const previewImage = (event) => {
    const fileImg = event.files[0];
    if (fileImg) {
        mainImageFile.value = fileImg;
        const reader = new FileReader();
        reader.onload = (e) => {
            imagePreview.value = e.target.result;
        };
        reader.readAsDataURL(fileImg);
    }
};

//搜索標籤用
function searchTags(event) {
    if (!event.query.trim().length) {
        tags.value = [...originalTags.value]; // 當沒有查詢時，顯示完整標籤列表
    } else {
        tags.value = originalTags.value.filter((tag) => {
            return tag.name.includes(event.query); // 模糊搜尋
        });
    }
}

const files = reactive([]); // 儲存上傳的文件的訊息
const handleFileChange = (event) => {
    const fileList = event.target.files;
    files.length = 0; // 清空之前的文件
    Array.from(fileList).forEach((file, index) => {
        const reader = new FileReader();
        reader.onload = (e) => {
            files.push({
                name: file.name,
                type: file.type,
                preview: e.target.result,
                file: file,
                sort: index
            });
        };
        reader.readAsDataURL(file);
    });

    // 重新初始化拖動功能
    nextTick(() => {
        const previewContainer = document.getElementById('media-preview');
        if (previewContainer) {
            Sortable.create(previewContainer, {
                animation: 150,
                ghostClass: 'sortable-ghost',
                handle: '.media-item',
                onEnd: updateMediaOrder
            });
        }
    });
};

// 刪除文件
const removeFile = (index) => {
    files.splice(index, 1); // 從列表中刪除文件
    updateSortValues();
};

// 更新文件順序
const updateMediaOrder = () => {
    const mediaOrder = Array.from(document.querySelectorAll('.media-item')).map((item) => item.dataset.index);

    // 根據新的順序更新文件的 sort 值
    mediaOrder.forEach((newIndex, i) => {
        const file = files.find((_, index) => index == newIndex);
        if (file) {
            file.sort = i; // 更新排序值
        }
    });
};

// 在刪除文件後重新給文件排序
const updateSortValues = () => {
    files.forEach((file, index) => {
        file.sort = index; // 根據當前文件順序更新 sort 值
    });
};

// 生成影片縮略圖
const generateThumbnail = (event, index) => {
    const video = event.target;
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
    const thumbnail = canvas.toDataURL();
    files[index].thumbnail = thumbnail; // 保存縮略圖
};

// 宣告需要的變數
const events = ref([]); // 儲存公告資料
const newEvent = ref({
    title: '',
    hint: '',
    date: '',
    description: '',
    image: null
});
const isModalOpen = ref(false); // 控制 Dialog 開關

// 打開 Dialog
function openEventModal() {
    isModalOpen.value = true;
}

// 關閉 Dialog
function closeModal() {
    isModalOpen.value = false;
}

function onFileChange(event) {
    const file = event.target.files[0];
    if (file) {
        newEvent.value.imageFile = file; // 保存文件
        const reader = new FileReader();
        reader.onload = (e) => {
            newEvent.value.image = e.target.result;
        };
        reader.readAsDataURL(file);
    }
}

const saveEvent = () => {
    if (!newEvent.value.title || !newEvent.value.date || !newEvent.value.description || !newEvent.value.image) {
        // alert('請填寫所有欄位並上傳圖片');
        toast.add({ severity: 'error', summary: '更新失敗', detail: '請填寫所有欄位並上傳圖片', life: 3000 });
        return;
    }

    // 確保日期為 Date 對象
    let eventDate;
    if (typeof newEvent.value.date === 'string' || !(newEvent.value.date instanceof Date)) {
        eventDate = new Date(newEvent.value.date);
    } else {
        eventDate = newEvent.value.date;
    }

    // 使用格式化的日期來代替原始的 Date 物件
    const formattedEvent = {
        ...newEvent.value,
        date: formatDate(eventDate) // 使用格式化后的日期
    };

    events.value.push(formattedEvent);

    // 重置 newEvent
    newEvent.value = {
        title: '',
        hint: '',
        date: '',
        description: '',
        image: null
    };

    closeModal(); // 保存後關閉 Dialog
};

// 格式化日期函数，返回 yyyy-MM-dd 格式
function formatDate(date) {
    const year = date.getFullYear();
    const month = ('0' + (date.getMonth() + 1)).slice(-2); // 確保月份兩位數
    const day = ('0' + date.getDate()).slice(-2); // 確保日期兩位數
    return `${year}-${month}-${day}`;
}

// 刪除指定公告卡片
function deleteEventCard(index) {
    events.value.splice(index, 1); // 移除指定索引的公告
}

const gameAbout = ref(''); // 儲存 "關於此遊戲" 的輸入值
const aboutImageBlocks = ref([]); // 儲存所有圖片描述區塊
let aboutImageCount = 0; // 用於給每個區塊唯一的 id

// 新增圖片描述區塊
function addImageBlock() {
    aboutImageCount++;
    aboutImageBlocks.value.push({
        id: aboutImageCount,
        preview: '', // 圖片預覽的 src
        title: '',
        description: ''
    });
}

function handleImageUpload(event, id) {
    const file = event.target.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = (e) => {
            const block = aboutImageBlocks.value.find((block) => block.id === id);
            if (block) {
                block.preview = e.target.result;
                block.imageFile = file; // 保存文件
            }
        };
        reader.readAsDataURL(file);
    }
}

// 刪除指定圖片描述區塊
function deleteImageBlock(id) {
    aboutImageBlocks.value = aboutImageBlocks.value.filter((block) => block.id !== id);
}

// 使用 markRaw 防止組件變為響應式對象
const tabs = [
    { name: 'Poor', label: '低等', component: markRaw(Poor) },
    { name: 'Average', label: '中等', component: markRaw(Average) },
    { name: 'Excellent', label: '高等', component: markRaw(Excellent) },
    { name: 'Super', label: '超高等', component: markRaw(Super) }
];

// 綁定當前選中的 tab
const currentTab = ref(tabs[0].name);
const currentComponent = computed(() => tabs.find((t) => t.name === currentTab.value)?.component);

// 引用動態組件的實例
const dynamicComponent = ref(null);

// 手動抓取當前組件的 HTML 結構
const getComponentHTML = async () => {
    await nextTick(); // 確保 DOM 已更新
    if (dynamicComponent.value) {
        const componentInstance = dynamicComponent.value.$el;
        return componentInstance.innerHTML; // 返回 innerHTML
    } else {
        toast.add({ severity: 'error', summary: '失敗', detail: '失敗', life: 3000 });
    }
};

const submitForm = async () => {
    submitted.value = true;

    const fields = [
        { field: files.length, message: '請上傳圖片' },
        { field: gameName.value, message: '請填寫遊戲名稱' },
        { field: selectedpublisher.value, message: '請選擇發行商' },
        { field: launchDate.value, message: '請選擇發行日期' },
        { field: mainImageFile.value, message: '請選擇遊戲主圖' },
        { field: selectedCategory.value, message: '請選擇遊戲類別' },
        { field: selectedTags.value, message: '請選擇遊戲標籤(可複選)' },
        { field: description.value, message: '請填寫遊戲描述' },
        { field: files, message: '請填寫遊戲描述' },
        { field: gameAbout.value, message: '請填寫關於此遊戲' }
    ];

    for (const { field, message } of fields) {
        if (!field) {
            toast.add({ severity: 'error', summary: '上架失敗', detail: message, life: 1500 });
            return;
        }
    }

    loading.value = true; // 設置為 true，表示開始提交

    const systemRequirementHTML = await getComponentHTML();

    try {
        // 準備產品資料物件
        const product = {
            ProductName: gameName.value,
            ProductPrice: gamePrice.value,
            ProductShelfTime: launchDate.value ? launchDate.value.toISOString() : '',
            Description: description.value,
            PublisherID: selectedpublisher.value.split(' ')[0], // 提取 PublisherID
            CategoryId: selectedCategory.value.split(' ')[0], // 提取 CategoryId
            SystemRequirement: systemRequirementHTML,
            ProductMainImageFile: mainImageFile.value, // 主圖片
            carouselImages: files.map((file, index) => ({
                file: file.file, // 輪播圖片文件
                index: file.sort // 輪播圖片的索引
            })),
            tags: selectedTags.value.map((tag) => ({ tagId: tag.id })), // 標籤 ID
            events: events.value.map((event) => ({
                Title: event.title,
                AnnounceText: event.hint,
                Content: event.description,
                AnnouncementDate: event.date || '',
                ProductImgFile: event.imageFile || '' // 公告圖片
            })),
            abouts: {
                AboutMainTitle: gameAbout.value,
                aboutCardListDtos: aboutImageBlocks.value.map((about) => ({
                    Title: about.title,
                    Text: about.description,
                    AboutCardImgFile: about.imageFile || '' // 關於圖片
                }))
            }
        };

        // 呼叫服務層 API 並傳送產品資料
        await ProductLaunchService.createProduct(product);
        toast.add({ severity: 'success', summary: '新增成功', detail: '產品上架成功，待管理員審核', life: 3000 });

        await setTimeout(() => {
            router.push({ name: 'AllProduct', query: { sortBy: 'productId', order: 'desc' } });
        }, 3000);
    } catch (error) {
        // alert('上架產品時發生錯誤');
        toast.add({ severity: 'error', summary: '新增失敗', detail: '產品上架失敗', life: 3000 });
    } finally {
        loading.value = false; // 請求結束後，將 loading 設置為 false
    }
};
</script>

<template>
    
    <Accordion value="0">
        <AccordionPanel value="0">
            <AccordionHeader>產品上架</AccordionHeader>
            <AccordionContent>
                <div class="flex flex-row">
                    <div class="w-7/12">
                        <div class="flex flex-row">
                            <div class="w-6/12 me-3">
                                <div class="mb-5">
                                    <label class="inline-block ps-5 w-4/12">
                                        遊戲名稱
                                        <small v-if="submitted && !gameName" class="text-red-500">必填</small>
                                    </label>
                                    <InputText v-model="gameName" placeholder="限制50字元" class="w-8/12" autofocus required="true" :invalid="submitted && !gameName" />
                                </div>
                                <div class="mb-5">
                                    <label class="inline-block ps-5 w-4/12">
                                        發行商
                                        <small v-if="submitted && !selectedpublisher" class="text-red-500">必填</small>
                                    </label>
                                    <Select v-model="selectedpublisher" searchable="true" :options="publishers" placeholder="選擇發行商" class="w-8/12" required="true" :invalid="submitted && !selectedpublisher"></Select>
                                </div>
                                <div class="mb-5">
                                    <label class="inline-block ps-5 w-4/12">
                                        發行日期
                                        <small v-if="submitted && !selectedpublisher" class="text-red-500">必填</small>
                                    </label>
                                    <DatePicker v-model="launchDate" :minDate="sevenDate" placeholder="審核遊戲須七日" :showIcon="true" :showButtonBar="true" class="w-8/12" required="true" :invalid="submitted && !launchDate" />
                                </div>
                                <div class="mb-5">
                                    <label class="inline-block ps-5 w-4/12">販售價格</label>
                                    <InputNumber v-model="gamePrice" class="w-8/12" />
                                </div>
                            </div>
                            <div class="w-6/12">
                                <div class="flex flex-col justify-center ps-5">
                                    <label class="block text-center">遊戲主圖</label>
                                    <FileUpload name="demo[]" mode="basic" @select="previewImage" accept="image/*" class="my-4" />
                                </div>
                                <div class="my-6">
                                    <label class="inline-block ps-5 w-4/12">
                                        遊戲類別
                                        <small v-if="submitted && !selectedCategory" class="text-red-500">必填</small>
                                    </label>
                                    <Select v-model="selectedCategory" :options="categories" placeholder="選擇遊戲類別" class="w-8/12" required="true" :invalid="submitted && !selectedCategory"></Select>
                                </div>
                                <div class="my-5">
                                    <label class="inline-block ps-5 w-4/12">
                                        遊戲標籤
                                        <small v-if="submitted && !selectedTags" class="text-red-500">必填</small>
                                    </label>
                                    <AutoComplete
                                        v-model="selectedTags"
                                        :suggestions="tags"
                                        optionLabel="name"
                                        placeholder="選擇遊戲標籤或輸入搜尋"
                                        dropdown
                                        multiple
                                        display="chip"
                                        @complete="searchTags($event)"
                                        class="w-8/12"
                                        required="true"
                                        :invalid="submitted && !selectedTags"
                                    />
                                </div>
                            </div>
                        </div>
                        <div class="flex flex-row">
                            <label class="flex items-center ps-5 w-2/12">
                                遊戲描述
                                <small v-if="submitted && !description" class="text-red-500">必填</small>
                            </label>
                            <textarea v-model="description" placeholder="Description" class="w-10/12" required="true" :invalid="submitted && !description"></textarea>
                        </div>
                    </div>
                    <div class="w-4/12 px-3">
                        <!-- 顯示圖片預覽 -->
                        <h4>圖片預覽:</h4>
                        <div v-if="imagePreview">
                            <img :src="imagePreview" style="max-width: 420px; aspect-ratio: 16 / 9" />
                        </div>
                    </div>
                </div>
            </AccordionContent>
        </AccordionPanel>
        <AccordionPanel value="1">
            <AccordionHeader>遊戲畫面</AccordionHeader>
            <AccordionContent>
                <div class="ps-12">
                    <div>
                        <label class="inline-block w-full">遊戲畫面(限10MB以下) 允許多選</label>
                        <input type="file" multiple @change="handleFileChange" accept="image/*" />
                        <!-- , video/* -->
                    </div>
                    <div id="media-preview" class="mt-4 flex flex-wrap flex-row">
                        <div v-for="(file, index) in files" :key="index" class="my-1 w-1/6 media-item" :data-index="index" :style="{ cursor: 'move' }">
                            <!-- 顯示圖片或影片預覽 -->
                            <img v-if="file.type.startsWith('image/')" :src="file.preview" style="max-width: 200px; aspect-ratio: 16/9" />
                            <video v-if="file.type.startsWith('video/')" :src="file.preview" style="max-width: 200px; aspect-ratio: 16/9" @loadeddata="generateThumbnail($event, index)"></video>
                            <!-- 檔案名稱 -->
                            <p class="mt-2 break-words">{{ file.name }}</p>
                            <!-- 刪除按鈕 -->
                            <button @click="removeFile(index)" class="mt-2 bg-red-500 text-white px-2 py-1 rounded">刪除</button>
                        </div>
                    </div>
                </div>
            </AccordionContent>
        </AccordionPanel>
        <AccordionPanel value="2">
            <AccordionHeader>遊戲公告</AccordionHeader>
            <AccordionContent>
                <div>
                    <div class="enentContainer">
                        <!-- 放置公告卡片的區域 -->
                        <div id="event-card-container" class="row">
                            <!-- 公告卡片會在這裡顯示 -->
                            <div v-for="(event, index) in events" :key="index" class="me-2">
                                <div class="each-card mb-4">
                                    <div class="card">
                                        <img :src="event.image" alt="公告圖示" style="width: 200px; object-fit: cover; aspect-ratio: 16/9" />
                                        <div class="card-img-overlay h-full">
                                            <h5 class="text-white">新聞</h5>
                                            <p>{{ event.description }}</p>
                                        </div>
                                    </div>
                                    <div class="p-2">
                                        <h5>{{ event.title }}</h5>
                                        <p>
                                            <small>{{ event.hint }}</small>
                                            <br />
                                            <small>{{ event.date }}</small>
                                        </p>
                                        <Button label="刪除公告" severity="danger" type="button" @click="deleteEventCard(index)"></Button>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- 按鈕觸發 Modal 彈窗 -->
                        <button id="open-modal-btn" class="btn-primary" @click="openEventModal">新增公告</button>

                        <Dialog v-model:visible="isModalOpen" header="新增公告" modal>
                            <div>
                                <div class="w-full">
                                    <label class="inline-block w-3/12">公告標題</label>
                                    <InputText v-model="newEvent.title" placeholder="限制50字元" class="w-6/12" />
                                </div>
                                <div class="w-full mt-4">
                                    <label class="inline-block w-3/12">預告詞</label>
                                    <InputText v-model="newEvent.hint" placeholder="限制50字元" class="w-6/12" />
                                </div>
                                <div class="w-full mt-4">
                                    <label class="inline-block w-3/12">公告日期</label>
                                    <DatePicker v-model="newEvent.date" :showIcon="true" :showButtonBar="true" :minDate="minDate" placeholder="公告日期不得低於今日" class="w-6/12" />
                                </div>
                                <div class="w-full mt-4">
                                    <label class="inline-block w-3/12">公告圖示(限10MB以下)</label>
                                    <input type="file" @change="onFileChange" accept="image/jpeg, image/png, image/webp" />
                                </div>
                                <div class="w-full mt-4">
                                    <label class="inline-block w-full">公告詳細描述</label>
                                    <textarea class="w-full" v-model="newEvent.description" placeholder="Description"></textarea>
                                </div>
                            </div>

                            <template #footer>
                                <Button label="取消" severity="secondary" @click="closeModal"></Button>
                                <Button label="保存公告" @click="saveEvent"></Button>
                            </template>
                        </Dialog>
                    </div>
                </div>
            </AccordionContent>
        </AccordionPanel>
        <AccordionPanel value="3">
            <AccordionHeader>關於此遊戲</AccordionHeader>
            <AccordionContent>
                <div>
                    <div>
                        <label class="inline-block w-full h-8 text-sm mt-3">
                            關於此遊戲
                            <small v-if="submitted && !gameAbout" class="text-red-500">必填</small>
                        </label>
                        <textarea v-model="gameAbout" placeholder="關於此遊戲的詳細描述" class="w-full" required="true" :invalid="submitted && !gameAbout"></textarea>
                    </div>
                    <!-- 圖片描述區塊 -->
                    <div id="image-section" class="mt-3 flex flex-row flex-wrap">
                        <div v-for="(imageBlock, index) in aboutImageBlocks" :key="imageBlock.id" class="p-2 w-3/12">
                            <div class="mb-2">
                                <img :id="`imageAboutPreview_${imageBlock.id}`" :src="imageBlock.preview" alt="請放遊戲描述圖預覽" style="width: 250px; aspect-ratio: 16/9" />
                            </div>
                            <div class="mb-2">
                                <label :for="`gameAboutupload_${imageBlock.id}`">遊戲描述圖</label>
                                <input type="file" :id="`gameAboutupload_${imageBlock.id}`" @change="handleImageUpload($event, imageBlock.id)" accept="image/gif, image/jpeg, image/png, image/webp" required class="w-full" />
                            </div>
                            <div class="mb-2">
                                <label :for="`gameAboutTitle_${imageBlock.id}`" class="inline-block w-3/12">圖片主題</label>
                                <InputText v-model="imageBlock.title" :id="`gameAboutTitle_${imageBlock.id}`" placeholder="限制100字元" />
                            </div>
                            <div>
                                <label :for="`gameDescription_${imageBlock.id}`" class="flex items-center">圖片描述</label>
                                <textarea v-model="imageBlock.description" :id="`gameDescription_${imageBlock.id}`" placeholder="Description" class="w-full"></textarea>
                            </div>
                            <Button label="刪除" severity="danger" @click="deleteImageBlock(imageBlock.id)"></Button>
                        </div>
                    </div>

                    <!-- 生成圖片描述區塊按鈕 -->
                    <button type="button" class="btn btn-primary mt-3" @click="addImageBlock">新增圖片描述</button>
                </div>
            </AccordionContent>
        </AccordionPanel>
        <AccordionPanel value="4">
            <AccordionHeader>系統需求</AccordionHeader>
            <AccordionContent>
                <div class="ps-5">
                    <!-- Tab 選擇按鈕區域 -->
                    <div class="w-full flex flex-row py-4">
                        <div v-for="tab in tabs" :key="tab.name" class="ms-6 mx-6">
                            <!-- 使用 v-model 綁定 currentTab -->
                            <input type="radio" :value="tab.name" v-model="currentTab" class="hidden peer" :id="'tab-' + tab.name" @change="getComponentHTML" />
                            <label
                                :for="'tab-' + tab.name"
                                class="inline-flex items-center justify-between w-full p-5 text-gray-500 bg-white border border-gray-200 rounded-lg cursor-pointer dark:hover:text-gray-300 dark:border-gray-700 dark:peer-checked:text-blue-500 peer-checked:border-blue-600 peer-checked:text-blue-600 hover:text-gray-600 hover:bg-gray-100 dark:text-gray-400 dark:bg-gray-800 dark:hover:bg-gray-700"
                            >
                                {{ tab.label }}
                            </label>
                        </div>
                    </div>

                    <!-- 渲染動態組件 -->
                    <div v-if="currentComponent">
                        <component :is="currentComponent" ref="dynamicComponent" />
                    </div>
                </div>
            </AccordionContent>
        </AccordionPanel>
    </Accordion>

    <div v-if="loading" class="loading-indicator">
        <div class="fixed loading-title">正在上架中，請稍候...</div>
        <div>
            <img src="https://res.cloudinary.com/dijn0xzac/image/upload/v1728299253/eofcjued0iellxzha0i9.gif" alt="" />
        </div>
    </div>
    <button type="button" class="btn btn-primary mt-3" @click="submitForm">點我上架</button>
</template>

<style>
/* Modal 覆蓋層 */
/* .custom-modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
} */

/* Modal */
/* .custom-modal {
    background: white;
    padding: 20px;
    border-radius: 8px;
    width: 500px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.2);
} */

/* .modal-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
} */

/* .modal-title {
    font-size: 1.25rem;
    margin: 0;
} */

/* .btn-close {
    background: none;
    border: none;
    font-size: 1.5rem;
    cursor: pointer;
} */

/* .modal-body {
    margin-top: 20px;
}

.modal-body label,
.modal-body input {
    height: 30px;
} */

/* .modal-footer {
    display: flex;
    justify-content: flex-end;
    gap: 10px;
    margin-top: 20px;
} */

/* 按鈕樣式 */
.btn-primary {
    background-color: #007bff;
    border: none;
    color: white;
    padding: 10px 20px;
    cursor: pointer;
    border-radius: 4px;
}

.btn-primary:hover {
    background-color: #0056b3;
}

/* .btn-secondary {
    background-color: #6c757d;
    border: none;
    color: white;
    padding: 10px 20px;
    cursor: pointer;
    border-radius: 4px;
}

.btn-secondary:hover {
    background-color: #5a6268;
} */

/* 卡片樣式 */
.row {
    display: flex;
    flex-wrap: wrap;
}

.each-card {
    border: 1px solid #ddd;
    border-radius: 8px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    overflow: hidden;
}

.card {
    position: relative;
}

.card-img-overlay {
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    background: rgba(0, 0, 0, 0.6);
    color: white;
    padding: 10px;
}

.each-card:hover::before {
    opacity: 1;
}

.each-card .card-img-overlay {
    position: absolute;
    opacity: 0;
    transition: opacity 0.3s ease;
    z-index: 2;
}

.each-card:hover .card-img-overlay {
    opacity: 1;
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
    z-index: 9; /* 確保它覆蓋其他內容 */
}
.loading-title {
    font-size: 20px;
    font-weight: bolder;
    color: #80c;
    left: 46%;
    top: 64%;
}
</style>
