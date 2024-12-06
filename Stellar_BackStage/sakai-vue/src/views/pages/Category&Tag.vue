<template>
    <div class="flex">
     
        <div class="card mr-2">
            <h2 class="text-[30px] py-2">Categories</h2>
            <div>
                <Toolbar class="mb-6">
                    <template #start>
                        <Button label="New" icon="pi pi-plus" severity="secondary" class="mr-2" @click="confirmAddCategory" />
                        <!-- <Button label="Delete" icon="pi pi-trash" severity="secondary" @click="confirmDeleteCategorySelected" :disabled="!selectedCategories.length" /> -->
                    </template>

                    <template #end>
                        <Button label="Export" icon="pi pi-upload" severity="secondary" @click="exportCSV($event)" />
                    </template>
                </Toolbar>

                <DataTable ref="dt" v-model:selection="selectedCategories" :value="categories" dataKey="categoryId" :paginator="true" :rows="10" :filters="categoryFilters"  :global-filter-fields="['categoryId', 'categoryName']" >
                    <template #header>
                        <div class="flex flex-wrap gap-2 items-center justify-between">
                           
                            <IconField>
                                <InputIcon>
                                    <i class="pi pi-search" />
                                </InputIcon>
                                <InputText v-model="categoryFilters['global'].value" placeholder="Search..." />
                            </IconField>
                        </div>
                    </template>

                    <!-- <Column selectionMode="multiple" style="width: 3rem" :exportable="false"></Column> -->
                    <Column field="categoryId" header="ID" sortable style="min-width: 12rem"></Column>
                    <Column field="categoryName" header="Name" sortable style="min-width: 16rem"></Column>
                    <Column header="Image">
                        <template #body="slotProps">
                            <img :src="slotProps.data.categoryImgUrl" :alt="slotProps.data.categoryName" class="rounded w-[64px] h-[64px]" style="object-fit: cover" />
                        </template>
                    </Column>

                    <Column :exportable="false" style="min-width: 12rem">
                        <template #body="slotProps">
                            <Button icon="pi pi-pencil" outlined rounded class="mr-2" @click="confirmModifyCategory(slotProps.data)" />
                            <Button icon="pi pi-trash" outlined rounded severity="danger" @click="confirmDeleteCategory(slotProps.data)" />
                        </template>
                    </Column>
                </DataTable>
            </div>

            <Dialog v-model:visible="addCategoryDialog" :style="{ width: '450px' }" header="Create Category" :modal="true">
                <div class="flex flex-col gap-6">
                    <div class="">
                        <label for="categoryName" class="block font-bold mb-3">CategoryName</label>
                        <InputText id="categoryName" v-model.trim="category.categoryName" required="true" autofocus :invalid="submitted && !category.categoryName" fluid />
                        <small v-if="submitted && !category.categoryName" class="text-red-500">Name is required.</small>
                    </div>
                </div>
                <div class="mt-2">
                    <label for="categoryImgUrl" class="block font-bold mb-3">Category Image</label>

                    <img v-if="previewImg" :src="previewImg" class="w-[120px] h-[80px] my-3" style="object-fit: cover" alt="Preview" />
                    <input type="file" @change="onFileChange" accept="image/*" />
                </div>

                <template #footer>
                    <Button label="Cancel" icon="pi pi-times" text @click="hideDialog" />
                    <Button label="Add" icon="pi pi-check" @click="addCategory" />
                </template>
            </Dialog>

            <Dialog v-model:visible="modifyCategoryDialog" :style="{ width: '450px' }" header="Modify Category" :modal="true">
                <div class="flex flex-col gap-6">
                    <img v-if="category && category.categoryImgUrl" :src="category.categoryImgUrl" :alt="category.categoryImgUrl" class="block m-auto w-[100px] pb-4" />
                    <div>
                        <label for="categoryName" class="block font-bold mb-3">CategoryName</label>
                        <InputText id="categoryName" v-model.trim="category.categoryName" required="true" autofocus :invalid="submitted && !category.categoryName" fluid />
                        <small v-if="submitted && !category.categoryName" class="text-red-500">Name is required.</small>
                    </div>

                    <div>
                        <label for="categoryImgUrl" class="block font-bold mb-3">Upload Image</label>
                        <img v-if="previewImg" :src="previewImg" class="w-[120px] h-[80px] my-3" style="object-fit: cover" alt="Preview" />
                        <input type="file" @change="onFileChange" accept="image/*" />
                    </div>
                </div>

                <template #footer>
                    <Button label="Cancel" icon="pi pi-times" text @click="hideDialog" />
                    <Button label="Save" icon="pi pi-check" @click="modifyCategory" />
                </template>
            </Dialog>

            <Dialog v-model:visible="deleteCategoryDialog" :style="{ width: '450px' }" header="Delete Category" :modal="true">
                <div class="flex items-center gap-4">
                    <i class="pi pi-exclamation-triangle !text-3xl" />
                    <span v-if="category"
                        >Are you sure you want to delete <b>{{ category.categoryName }}</b
                        >?</span
                    >
                </div>
                <template #footer>
                    <Button label="No" icon="pi pi-times" text @click="deleteCategoryDialog = false" />
                    <Button label="Yes" icon="pi pi-check" @click="removeCategory()" />
                </template>
            </Dialog>

            <Dialog v-model:visible="deleteCategoriesDialog" :style="{ width: '450px' }" header="Confirm" :modal="true">
                <div class="flex items-center gap-4">
                    <i class="pi pi-exclamation-triangle !text-3xl" />
                    <span v-if="product"
                        >Are you sure you want to delete <b>{{ category.categoryName }}</b
                        >?</span
                    >
                </div>
                <template #footer>
                    <Button label="No" icon="pi pi-times" text @click="deleteCategoriesDialog = false" />
                    <Button label="Yes" icon="pi pi-check" @click="removeCategory" />
                </template>
            </Dialog>
            <div v-if="categoryrErorMessage" class="text-red-500">{{ categoryrErorMessage }}</div>
        </div>

      
        <div class="card ml-2">
            <h2 class="text-[30px] py-2">Tags</h2>
            <div>
                <Toolbar class="mb-6">
                    <template #start>
                        <Button label="New" icon="pi pi-plus" severity="secondary" class="mr-2" @click="confirmAddTagCategory" />
                        <!-- <Button label="Delete" icon="pi pi-trash" severity="secondary" @click="confirmDeleteTagCategory" :disabled="!selectedTagies || !selectedTagies.length" /> -->
                    </template>

                    <template #end>
                        <Button label="Export" icon="pi pi-upload" severity="secondary" @click="exportCSV($event)" />
                    </template>
                </Toolbar>

                <DataTable ref="dt" v-model:selection="selectedTagies" :value="tagies" dataKey="tagId" :paginator="true" :rows="10" :filters="tagFilters">
                    <template #header>
                        <div class="flex flex-wrap gap-2 items-center justify-between">
                            <IconField>
                                <InputIcon>
                                    <i class="pi pi-search" />
                                </InputIcon>
                                <InputText v-model="tagFilters['global'].value" placeholder="Search..." />
                            </IconField>
                        </div>
                    </template>

                    <!-- <Column selectionMode="multiple" style="width: 3rem" :exportable="false"></Column> -->
                    <Column field="tagId" header="ID" sortable style="min-width: 12rem"></Column>
                    <Column field="tagName" header="Name" sortable style="min-width: 16rem"></Column>

                    <Column :exportable="false" style="min-width: 12rem">
                        <template #body="slotProps">
                            <Button icon="pi pi-pencil" outlined rounded class="mr-2" @click="confirmmodifyTagCategory(slotProps.data)" />
                            <Button icon="pi pi-trash" outlined rounded severity="danger" @click="confirmDeleteTagCategory(slotProps.data)" />
                        </template>
                    </Column>
                </DataTable>
            </div>
            <Dialog v-model:visible="addtagDialog" :style="{ width: '450px' }" header="Create Tag" :modal="true">
                <div class="flex flex-col gap-6">
                    <div>
                        <label for="tagName" class="block font-bold mb-3">Tag Name</label>
                        <InputText id="tagName" v-model.trim="tag.tagName" required="true" autofocus :invalid="submitted && !tag.tagName" fluid />
                        <small v-if="submitted && !tag.tagName" class="text-red-500">Name is required.</small>
                    </div>
                </div>

                <template #footer>
                    <Button label="Cancel" icon="pi pi-times" text @click="hideDialog" />
                    <Button label="Add" icon="pi pi-check" @click="addTag" />
                </template>
            </Dialog>

            <Dialog v-model:visible="modifyTagDialog" :style="{ width: '450px' }" header="Modify Tag" :modal="true">
                <div class="flex flex-col gap-6">
                    <div>
                        <label for="name" class="block font-bold mb-3">Name</label>
                        <InputText id="name" v-model.trim="tag.tagName" required="true" autofocus :invalid="submitted && !tag.tagName" fluid />
                        <small v-if="submitted && !tag.tagName" class="text-red-500">Name is required.</small>
                    </div>
                </div>

                <template #footer>
                    <Button label="Cancel" icon="pi pi-times" text @click="hideDialog" />
                    <Button label="Save" icon="pi pi-check" @click="modifyTag" />
                </template>
            </Dialog>

            <Dialog v-model:visible="deleteTagDialog" :style="{ width: '450px' }" header="Delete Tag" :modal="true">
                <div class="flex items-center gap-4">
                    <i class="pi pi-exclamation-triangle !text-3xl" />
                    <span v-if="tag"
                        >Are you sure you want to delete <b>{{ tag.tagName }}</b
                        >?</span
                    >
                </div>
                <template #footer>
                    <Button label="No" icon="pi pi-times" text @click="hideDialog = false" />
                    <Button label="Yes" icon="pi pi-check" @click="removeTag" />
                </template>
            </Dialog>
            <div v-if="tagErrorMessage" class="text-red-500">{{ tagErrorMessage }}</div>
        </div>
    </div>
</template>

<script setup>
import { FilterMatchMode } from '@primevue/core/api';
import { useToast } from 'primevue/usetoast';

import { TagService } from '@/service/TagService';

import { CategoryService } from '@/service/CategoryService';
import { ref, onMounted } from 'vue';

const toast = useToast();
const dt = ref();


const categoryFilters = ref({
      global: { value: null, matchMode: FilterMatchMode.CONTAINS }
    });

    const tagFilters = ref({
      global: { value: null, matchMode: FilterMatchMode.CONTAINS }
    });

const filters = ref({
    global: { value: null, matchMode: FilterMatchMode.CONTAINS }
   
   
});
const submitted = ref(false);

function exportCSV() {
    dt.value.exportCSV();
}

const categories = ref([]);
const category = ref({});
const categoryrErorMessage = ref('');

const selectedCategories = ref([]);
const addCategoryDialog = ref(false);

const deleteCategoryDialog = ref(false);

const modifyCategoryDialog = ref(false);
const deleteCategoriesDialog = ref(false);

const isEditing = ref(false);

const confirmAddCategory = () => {
    isEditing.value = false;
    category.value = {};
    addCategoryDialog.value = true;
};

const confirmModifyCategory = (categoryToModify) => {
    category.value = categoryToModify;
    modifyCategoryDialog.value = true;
};

const confirmDeleteCategory = (categoryToDelete) => {
    category.value = categoryToDelete;
    deleteCategoryDialog.value = true;
};

const confirmDeleteCategorySelected = () => {
    if (selectedCategories.value.length) {
        category.value = selectedCategories.value[0];
        deleteCategoryDialog.value = true;
    }
};

const fetchCategories = async () => {
    try {
        categories.value = await CategoryService.fetchCategorys();
    } catch (error) {
        categoryrErorMessage.value = error.message;
    }
};

const addCategory = async () => {
    submitted.value = true;
    if (!category.value.categoryName) {
        return;
    }

    const formData = new FormData();

    formData.append('categoryName', category.value.categoryName);
    formData.append('photoToCloudinary', category.value.photoToCloudinary);

    try {
        await CategoryService.addCategory(formData);
        await fetchCategories();
        toast.add({ severity: 'success', summary: 'Successful', detail: 'category added', life: 3000 });
        hideDialog();
    } catch (error) {
        categoryrErorMessage.value = error.message;
    }
};

const modifyCategory = async () => {
    if (!category.value.categoryName) {
        return;
    }

    const formData = new FormData();
    formData.append('categoryId', category.value.categoryId);
    formData.append('categoryName', category.value.categoryName);
    if (category.value.photoToCloudinary) {
        formData.append('photoToCloudinary', category.value.photoToCloudinary); // 將 File 對象直接添加
    }
    try {
        // await CategoryService.modifyCategory(category.value);
        await CategoryService.modifyCategory(formData);
        await fetchCategories();
        toast.add({ severity: 'success', summary: 'Successful', detail: 'category updated', life: 3000 });
        hideDialog();
    } catch (error) {
        categoryrErorMessage.value = error.message;
    }
};

const removeCategory = async () => {
    try {
        await CategoryService.removeCategory(category.value.categoryId);
        await fetchCategories();
        toast.add({ severity: 'success', summary: 'Successful', detail: 'category deleted', life: 3000 });
        hideDialog();
    } catch (error) {
        categoryrErorMessage.value = error.message;
    }
};



const previewImg = ref('');

const onFileChange = (event) => {
    const file = event.target.files[0];

    if (file) {
        const reader = new FileReader();
        reader.onload = (e) => {
            previewImg.value = e.target.result;
        };
        reader.readAsDataURL(file);
        category.value.photoToCloudinary = file;
    }
};

const hideDialog = () => {
    addCategoryDialog.value = false;
    deleteCategoryDialog.value = false;
    modifyCategoryDialog.value = false;
    categoryrErorMessage.value = '';

    addtagDialog.value = false;
    modifyTagDialog.value = false;
    deleteTagDialog.value = false;
};

onMounted(() => {
    fetchCategories();
});

//=========================================
const tagies = ref([]);
const tag = ref({});

const selectedTagies = ref([]);

const addtagDialog = ref(false);

const modifyTagDialog = ref(false);

const deleteTagDialog = ref(false);
// const deleteTagiesDialog = ref(false);

const tagErrorMessage = ref('');

const confirmAddTagCategory = () => {
    isEditing.value = false;
    tag.value = {};
    addtagDialog.value = true;
};

const confirmmodifyTagCategory = (tagToModify) => {
    tag.value = tagToModify;

    modifyTagDialog.value = true;
};

const confirmDeleteTagCategory = (tagToDelete) => {
    tag.value = tagToDelete;

    deleteTagDialog.value = true;
};

const fetchTags = async () => {
    try {
        tagies.value = await TagService.fetchTags();
    } catch (error) {
        tagErrorMessage.value = error.message;
    }
};

const addTag = async () => {
    submitted.value = true;
    console.log(tag.value.tagName);
    if (!tag.value.tagName) {
        return;
    }
    try {
        await TagService.addTags(tag.value);
        await fetchTags();
        toast.add({ severity: 'success', summary: 'Successful', detail: 'tag added', life: 3000 });
        hideDialog();
    } catch (error) {
        tagErrorMessage.value = error.message;
    }
};

const modifyTag = async () => {
    console.log(tag);
    if (!tag.value.tagName) {
        return;
    }

    try {
        await TagService.modifyTag(tag.value);
        await fetchTags();
        toast.add({ severity: 'success', summary: 'Successful', detail: 'tag Updated', life: 3000 });
        hideDialog();
    } catch (error) {
        tagErrorMessage.value = error.message;
    }
};

const removeTag = async () => {
    try {
        await TagService.removeTag(tag.value.tagId);
        await fetchTags();
        toast.add({ severity: 'success', summary: 'Successful', detail: 'tag deteled', life: 3000 });
        hideDialog();
    } catch (error) {
        tagErrorMessage.value = error.message;
    }
};

onMounted(() => {
    fetchTags();
});
</script>

<style src="../../output.css"></style>
