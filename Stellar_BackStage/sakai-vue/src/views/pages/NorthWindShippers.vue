<script setup>
import {ref , onMounted } from 'vue';
import { ProductService } from "@/service/ProductService";
import { ShipperService } from "@/service/ShippersService";


const ShipperDialog = ref(false);

const shippers = ref(null);
const shipper = ref({})
const submitted = ref(false);
const deleteShipperDialog = ref(false);

onMounted(()=>{
    ShipperService.getShippers().then((data)=>(shippers.value = data));
})

function hideDialog(){
    ShipperDialog.value=false;
    submitted.value=false;
}
function saveShipper(){
    submitted.value=true;
    console.assert.log('準備更新Shipper!')
    //todo 串接API
}

function editShipper(data){
    shipper.value = {...data};
    // shipper.value = data;
    ShipperDialog.value=true;
}

function confirmDeleteShipper(data){
    deleteShipperDialog.value=true;
    console.log(data);
    shipper.value={...data};
     
}

function deleteShipper(){
    console.log('準備刪除shipper!');
    //todo 串接API
    console.log('刪掉shipper');
    console.log(shipper.value);
    deleteShipperDialog.value=false;
}

</script>



<template>
    <div className="card">
        <DataTable :value="shippers" tableStyle="min-width: 50rem">
                <Column field="shipperId" header="ID" sortable></Column>
                <Column field="companyName" header="公司名稱"></Column>
                <Column field="phone" header="電話"></Column>

                <Column :exportable="false" style="min-width: 12rem">
                    <template #body="slotProps">
                        <Button icon="pi pi-pencil" outlined rounded class="mr-2" @click="editShipper(slotProps.data)" />
                        <Button icon="pi pi-trash" outlined rounded severity="danger" @click="confirmDeleteShipper(slotProps.data)" />
                    </template>
                </Column>
        </DataTable>
        <Dialog v-model:visible="ShipperDialog" :style="{ width: '450px' }" header="Shipper Details" :modal="true">
            <div class="flex flex-col gap-6">
                <div>
                    <label for="shipperId" class="block font-bold mb-3">ID</label>
                    <InputText id="shipperId" v-model.trim="shipper.shipperId" :invalid="submitted && !shipper.shipperId"   disabled="true" fluid />
                    
                </div>
                <div>
                    <label for="name" class="block font-bold mb-3">公司名</label>
                    <InputText id="name" v-model.trim="shipper.companyName" required="true" autofocus :invalid="submitted && !shipper.companyName" fluid />
                    <small v-if="submitted && !shipper.companyName" class="text-red-500">公司名稱必填</small>
                </div>
                <div>
                    <label for="phone" class="block font-bold mb-3">電話</label>
                    <InputText id="name" v-model.trim="shipper.phone" required="true"  :invalid="submitted && !shipper.phone" fluid />
                    <small v-if="submitted && !shipper.phone" class="text-red-500">電話為必填</small>
                </div>
            </div>

            <template #footer>
                <Button label="Cancel" icon="pi pi-times" text @click="hideDialog" />
                <Button label="Save" icon="pi pi-check" @click="saveShipper" />
            </template>
        </Dialog>
        <Dialog v-model:visible="deleteShipperDialog" :style="{ width: '450px' }" header="Confirm" :modal="true">
            <div class="flex items-center gap-4">
                <i class="pi pi-exclamation-triangle !text-3xl" />
                <span v-if="shipper">Are you sure you want to delete<b>{{ shipper.companyName }}</b>>?</span>
            </div>
            <template #footer>
                <Button label="No" icon="pi pi-times" text @click="deleteShipperDialog = false" />
                <Button label="Yes" icon="pi pi-check" text @click="deleteShipper" />
            </template>
        </Dialog>
    </div>
</template>