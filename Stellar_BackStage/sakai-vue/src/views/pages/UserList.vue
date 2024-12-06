<script setup>
import { getAllUserService } from '@/service/UserService'; // 导入服务
import { FilterMatchMode } from '@primevue/core/api';
import { useToast } from 'primevue/usetoast';
import { onMounted, ref } from 'vue';

onMounted(() => {
    getAllUserService.getUser().then((data) => (users.value = data));

});

const toast = useToast();
const dt = ref();
const users = ref();
const userDialog = ref(false);
const deleteUserDialog = ref(false);
const deleteUsersDialog = ref(false);
const user = ref({});
const selectedUsers = ref();
const filters = ref({
    global: { value: null, matchMode: FilterMatchMode.CONTAINS }
});
const submitted = ref(false);
const statuses = ref([
    { label: 'INSTOCK', value: 'instock' },
    { label: 'LOWSTOCK', value: 'lowstock' },
    { label: 'OUTOFSTOCK', value: 'outofstock' }
]);

// 打开新用户对话框
function openNew() {
    user.value = {};
    submitted.value = false;
    userDialog.value = true;
}

// 关闭用户对话框
function hideDialog() {
    userDialog.value = false;
    submitted.value = false;
}

// 保存用户信息
function saveUser() {
    getAllUserService.editUser(user.value.userId, user.value.nickName, user.value.email, user.value.country)
        .then(() => {
            userDialog.value = false;
            submitted.value = false;
            toast.add({ severity: 'success', summary: 'Successful', detail: 'edit success', life: 3000 });
        })
        .catch(() => {
            toast.add({ severity: 'error', summary: 'Error', detail: 'edit Failed', life: 3000 });
        });
}

// 编辑用户信息
function editUser(prod) {
    user.value = { ...prod };
    userDialog.value = true;
}

// 删除确认对话框
function confirmDeleteUser(prod) {
    user.value = prod;
    deleteUserDialog.value = true;

}

// "删除"用户时将islock设置为0
function deleteUser(user) {

    if (user.isLock === 0) {

        console.log("0");
        user.isLock = 1;
        // user.value.islock = 0; // 将 islock 设置为 0
        getAllUserService.updateUser(user.userId, user.isLock) // 使用 updateUser API 更新用户
            .then(() => {
                deleteUserDialog.value = false;
                toast.add({ severity: 'success', summary: 'Successful', detail: 'User Locked', life: 3000 });
            })
            .catch(() => {
                toast.add({ severity: 'error', summary: 'Error', detail: 'Lock Failed', life: 3000 });
            });

    } else if (user.isLock === 1) {
        console.log("1");
        user.isLock = 0;
        // user.value.islock = 0; // 将 islock 设置为 0
        getAllUserService.updateUser(user.userId, user.isLock) // 使用 updateUser API 更新用户
            .then(() => {
                deleteUserDialog.value = false;
                toast.add({ severity: 'success', summary: 'Successful', detail: 'User Locked', life: 3000 });
            })
            .catch(() => {
                toast.add({ severity: 'error', summary: 'Error', detail: 'Lock Failed', life: 3000 });
            });
    }

}

// 根据 ID 找到用户索引
function findIndexById(id) {
    let index = -1;
    for (let i = 0; i < users.value.length; i++) {
        if (users.value[i].id === id) {
            index = i;
            break;
        }
    }
    return index;
}

// 删除选择的用户确认对话框
function confirmDeleteSelected() {
    deleteUsersDialog.value = true;
}

// 批量"删除"用户时将islock设置为0
function deleteSelectedUsers() {
    selectedUsers.value.forEach(user => {
        user.islock = 0; // 将 islock 设置为 0
        getAllUserService.updateUser(user.id, user); // 批量更新用户
    });

    deleteUsersDialog.value = false;
    selectedUsers.value = null;
    toast.add({ severity: 'success', summary: 'Successful', detail: 'Users Locked', life: 3000 });
}

// 获取状态标签颜色
function getStatusLabel(status) {
    switch (status) {
        case 'INSTOCK':
            return 'success';
        case 'LOWSTOCK':
            return 'warn';
        case 'OUTOFSTOCK':
            return 'danger';
        default:
            return null;
    }
}
// if(users.value.isLock=1){users.value.islock="鎖定"}
console.dir(users);
</script>

<template>
    <div>
        <div class="card">

            <DataTable ref="dt" v-model:selection="selectedUsers" :value="users" dataKey="id" :paginator="true"
                :rows="10" :filters="filters"
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                :rowsPerPageOptions="[5, 10, 25]"
                currentPageReportTemplate="Showing {first} to {last} of {totalRecords} Users">
                <template #header>
                    <div class="flex flex-wrap gap-2 items-center justify-between">
                        <h4 class="m-0">UserList</h4>
                        <IconField>
                            <InputIcon>
                                <i class="pi pi-search" />
                            </InputIcon>
                            <InputText v-model="filters['global'].value" placeholder="Search..." />
                        </IconField>
                    </div>
                </template>

                <Column field="userId" header="ID" sortable style="min-width: 3rem"></Column>
                <Column field="nickName" header="nickName" sortable style="min-width: 10rem"></Column>
                <Column field="account" header="account" sortable style="min-width: 10rem"></Column>
                <Column field="wallet" header="wallet" sortable style="min-width: 4rem"></Column>
                <Column field="country" header="country" sortable style="min-width: 6rem"></Column>
                <Column field="email" header="email" sortable style="min-width: 16rem"></Column>
                <Column field="isLock" header="帳戶狀態" sortable style="min-width: 8rem">
                    <template #body="{ data }">
                        <span>{{ data.isLock === 1 ? '正常' : '封鎖' }}</span>
                    </template>
                </Column>

                <Column :exportable="false" style="min-width: 6rem">
                    <template #body="slotProps">
                        <Button icon="pi pi-pencil" outlined rounded class="mr-2" @click="editUser(slotProps.data)" />
                        <Button icon="pi pi-trash" outlined rounded severity="danger"
                            @click="confirmDeleteUser(slotProps.data)" />
                    </template>
                </Column>
            </DataTable>
        </div>

        <!-- 编辑用户的对话框 -->
        <Dialog v-model:visible="userDialog" :style="{ width: '450px' }" header="User Details" :modal="true">
            <div class="flex flex-col gap-6">

                <div>
                    <label for="nickName" class="block font-bold mb-3">nickName</label>
                    <InputText id="nickName" v-model.trim="user.nickName" required="true" autofocus
                        :invalid="submitted && !user.nickName" fluid />
                    <small v-if="submitted && !user.nickName" class="text-red-500">Name is required.</small>
                </div>
                <div>
                    <label for="nickNcountryame" class="block font-bold mb-3">country</label>
                    <InputText id="country" v-model.trim="user.country" required="true" autofocus
                        :invalid="submitted && !user.country" fluid />
                    <small v-if="submitted && !user.country" class="text-red-500">country is required.</small>
                </div>
                <div>
                    <label for="email" class="block font-bold mb-3">email</label>
                    <InputText id="email" v-model.trim="user.email" required="true" autofocus
                        :invalid="submitted && !user.email" fluid />
                    <small v-if="submitted && !user.email" class="text-red-500">email is required.</small>
                </div>
            </div>


            <template #footer>
                <Button label="Cancel" icon="pi pi-times" text @click="hideDialog" />
                <Button label="Save" icon="pi pi-check" @click="saveUser" />
            </template>
        </Dialog>

        <!-- 鎖定单个用户的确认对话框 -->
        <Dialog v-model:visible="deleteUserDialog" :style="{ width: '450px' }" header="Confirm" :modal="true">
            <div class="flex items-center gap-4">
                <i class="pi pi-exclamation-triangle !text-3xl" />
                <span v-if="user">是否切換 <b>{{ user.nickName }}</b>帳戶封鎖狀態?</span>
            </div>
            <template #footer>
                <Button label="No" icon="pi pi-times" text @click="deleteUserDialog = false" />
                <Button v-if="user" label="Yes" icon="pi pi-check" @click="deleteUser(user)" />
            </template>
        </Dialog>

    </div>
</template>
