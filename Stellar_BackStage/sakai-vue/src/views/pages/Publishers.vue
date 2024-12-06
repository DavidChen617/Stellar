<script setup>
import { ref, onMounted } from 'vue';

import { FilterMatchMode, FilterOperator } from '@primevue/core/api';
import { PublisherService } from '@/service/PublisherService';
const dt = ref();
const publisherDialog = ref(false);
const publishers = ref(null);
const publisher = ref({});
const submitted = ref(false);
const deletePublisherDialog = ref(false);
const selectedPublishers = ref();
const filters = ref({
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    status: { value: [], matchMode: FilterMatchMode.IN }
});
const selectedStatuses = ref([]);
const statusOptions = ref([
    { label: '活動中', value: '活動中' },
    { label: '停權中', value: '停權中' }
]);

const isEditMode = ref(false);
const loading = ref(false);

//狀態下拉式選單
const statusDropdownValues = ref([
    { name: '活動中', code: '活動中' },
    { name: '停權中', code: '停權中' }
]);

onMounted(() => {
    PublisherService.getPublishers().then((data) => (publishers.value = data));
    //console.log(publishers);
});
//渲染取得資料
async function fetchPublishers() {
    try {
        const response = await PublisherService.getPublishers();
        //console.log(response);
        publishers.value = response;
    } catch (error) {
        console.error('無法獲取資料', error);
    }
}

//關閉Modal
function hideDialog() {
    publisherDialog.value = false;
    submitted.value = false;
}

//MODAL的儲存
async function savepublisher() {
    submitted.value = true;

    try {
        if (publisher.value.address && publisher.value.concatName && publisher.value.country && publisher.value.phone) {
            //todo 串接API
            await PublisherService.updatePublisher(publisher.value);
            await fetchPublishers();
            publisherDialog.value = false;
        }
    } catch (error) {
        console.error('更新失敗', error);
    }
}
//打開編輯MODAL
function editpublisher(data) {
    publisher.value = { ...data };
    isEditMode.value = true;
    // publisher.value = data;
    publisherDialog.value = true;
}

function getStatusClass(status) {
    switch (status) {
        case '活動中':
            return 'pi-check-circle text-green-500';
        case '停權中':
            return 'pi-times-circle text-red-500';
        default:
            return '';
    }
}

function openNew() {
    publisher.value = {};
    submitted.value = false;
    isEditMode.value = false;
    publisherDialog.value = true;
}

function updateStatusFilter() {
    // 將選擇的狀態賦值給 filters 中的 status 篩選條件
    filters.value.status.value = selectedStatuses.value.length > 0 ? selectedStatuses.value : null;
}
</script>

<template>
    <div className="card">
        <Toolbar class="mb-6">
            <template #start>
                <Button label="New" icon="pi pi-plus" severity="secondary" class="mr-2" @click="openNew" />
            </template>
        </Toolbar>
        <DataTable
            :value="publishers"
            tableStyle="min-width: 50rem"
            ref="dt"
            v-model:selection="selectedPublishers"
            ataKey="id"
            :paginator="true"
            :rows="10"
            :filters="filters"
            filterDisplay="menu"
            :loading="loading"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        >
            <template #header>
                <div class="flex flex-wrap gap-2 items-center justify-between">
                    <h4 class="m-0">供應商管理</h4>
                    <IconField>
                        <InputIcon>
                            <i class="pi pi-search" />
                        </InputIcon>
                        <InputText v-model="filters['global'].value" placeholder="Search..." />
                    </IconField>
                </div>
            </template>
            <Column field="publisherId" header="供應商ID" sortable></Column>
            <!-- <Column field="publisherMainImageUrl" header="遊戲圖片"></Column> -->
            <Column field="publisherName" header="供應商名稱"></Column>
            <Column field="concatName" header="聯絡人"></Column>
            <Column field="country" header="國家" sortable></Column>
            <Column field="address" header="地址" sortable></Column>
            <Column field="phone" header="聯絡電話"></Column>
            <Column field="status" header="狀態" dataType="boolean" bodyClass="text-center" style="min-width: 8rem">
                <template #body="{ data }">
                    <i class="pi" :class="getStatusClass(data.status)"></i>
                </template>
                <template #filter="{ filterModel }">
                    <label for="status-filter" class="font-bold"> status </label>
                    <!-- <Checkbox v-model="filterModel.value" :indeterminate="filterModel.value == '停權中'" binary inputId="status-filter"  /> -->
                    <div v-for="option in statusOptions" :key="option.value" class="flex items-center gap-2">
                        <Checkbox v-model="selectedStatuses" :value="option.value" @change="updateStatusFilter" />
                        <label class="ml-2">
                            {{ option.label }}
                        </label>
                    </div>
                </template>
            </Column>
            <Column :exportable="false" style="min-width: 12rem" header="編輯">
                <template #body="slotProps">
                    <Button icon="pi pi-pencil" outlined rounded class="mr-2" @click="editpublisher(slotProps.data)" />
                </template>
            </Column>
        </DataTable>
        <Dialog v-model:visible="publisherDialog" :style="{ width: '450px' }" header="publisher Details" :modal="true">
            <div class="flex flex-col gap-6">
                <div>
                    <label for="publisherId" class="block font-bold mb-3">ID</label>
                    <InputText id="publisherId" v-model.trim="publisher.publisherId" disabled="true" fluid />
                </div>
                <div>
                    <label for="publisherName" class="block font-bold mb-3">供應商名稱</label>
                    <InputText id="publisherName" v-model.trim="publisher.publisherName" :disabled="isEditMode" autofocus :invalid="submitted && !publisher.publisherName" fluid />
                    <small v-if="submitted && !publisher.publisherName" class="text-red-500">供應商名稱必填</small>
                </div>
                <div>
                    <label for="concatName" class="block font-bold mb-3">聯絡人</label>
                    <InputText id="concatName" v-model.trim="publisher.concatName" required="true" :invalid="submitted && !publisher.concatName" fluid />
                    <small v-if="submitted && !publisher.concatName" class="text-red-500">聯絡人必填</small>
                </div>
                <div>
                    <label for="country" class="block font-bold mb-3">國家</label>
                    <InputText id="country" v-model.trim="publisher.country" required="true" :invalid="submitted && !publisher.country" fluid />
                    <small v-if="submitted && !publisher.country" class="text-red-500">國家必填</small>
                </div>
                <div>
                    <label for="address" class="block font-bold mb-3">地址</label>
                    <InputText id="address" v-model.trim="publisher.address" required="true" :invalid="submitted && !publisher.address" fluid />
                    <small v-if="submitted && !publisher.address" class="text-red-500">地址必填</small>
                </div>
                <div>
                    <label for="phone" class="block font-bold mb-3">聯絡電話</label>
                    <InputText id="phone" v-model.trim="publisher.phone" required="true" :invalid="submitted && !publisher.phone" fluid />
                    <!-- <InputMask id="phone" v-model.trim="publisher.phone" required="true" :invalid="submitted && !publisher.phone" mask="(99) 9999-9999" placeholder="(99) 9999-9999" fluid /> -->
                    <small v-if="submitted && !publisher.phone" class="text-red-500">聯絡電話必填</small>
                </div>
                <div>
                    <label for="status" class="block font-bold mb-3">供應商狀態</label>
                    <Select id="status" v-model="publisher.status" :options="statusDropdownValues" optionLabel="name" optionValue="code" placeholder="供應商狀態" />
                </div>
            </div>

            <template #footer>
                <Button label="Cancel" icon="pi pi-times" text @click="hideDialog" />
                <Button label="Save" icon="pi pi-check" @click="savepublisher" />
            </template>
        </Dialog>
    </div>
</template>
