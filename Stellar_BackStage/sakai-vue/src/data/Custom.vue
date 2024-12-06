<template>
    <div>
        <div class="system-request">
            <div class="responsive_block_header responsive_apppage_details_left create-bottom-border">自訂系統需求</div>
            <div class="sysreq_contents">
                addMinRequirement
                <div class="game_area_sys_req sysreq_content active d-flex row" data-os="win">
                    <div class="game_area_sys_req_leftCol col-12 col-md-6">
                        <ul class="cancel-padding">
                            <strong class="small-text">最低配備:</strong>
                            <br />
                            <ul class="bb_ul">
                                <li>需要 64 位元的處理器及作業系統<br /></li>
                                <li><strong>作業系統:</strong><input type="text" v-model="minSystem" placeholder="輸入最低配備作業系統" class="ms-3 mb-1" /><br /></li>
                                <li><strong>處理器:</strong><input type="text" v-model="minCPU" placeholder="輸入最低配備處理器" class="ms-3 mb-1" /><br /></li>
                                <li><strong>記憶體:</strong><input type="text" v-model="minRAM" placeholder="輸入最低配備記憶體" class="ms-3 mb-1" /><br /></li>
                                <li><strong>顯示卡:</strong><input type="text" v-model="minGPU" placeholder="輸入最低配備顯示卡" class="ms-3 mb-1" /><br /></li>
                                <li><strong>DirectX:</strong><input type="text" v-model="minDirectX" placeholder="輸入最低配備DirectX" class="ms-3 mb-1" /><br /></li>
                                <li><strong>儲存空間:</strong><input type="text" v-model="minROM" placeholder="輸入最低配備儲存空間" class="ms-3 mb-1" /><br /></li>
                                <li><strong>音效卡:</strong><input type="text" v-model="minSoundCard" placeholder="輸入最低配備音效卡" class="ms-3 mb-1" /><br /></li>
                                <li><strong>備註:</strong><input type="text" v-model="minRemark" placeholder="輸入最低配備備註" class="ms-3 mb-1" /></li>
                            </ul>
                        </ul>
                    </div>
                    <div class="game_area_sys_req_rightCol col-12 col-md-6">
                        <ul class="cancel-padding">
                            <strong class="small-text">建議配備:</strong>
                            <br />
                            <ul class="bb_ul">
                                <li>需要 64 位元的處理器及作業系統<br /></li>
                                <li><strong>作業系統:</strong><input type="text" v-model="maxSystem" placeholder="輸入建議配備作業系統" class="ms-3 mb-1" /><br /></li>
                                <li><strong>處理器:</strong><input type="text" v-model="maxCPU" placeholder="輸入建議配備處理器" class="ms-3 mb-1" /><br /></li>
                                <li><strong>記憶體:</strong><input type="text" v-model="maxRAM" placeholder="輸入建議配備記憶體" class="ms-3 mb-1" /><br /></li>
                                <li><strong>顯示卡:</strong><input type="text" v-model="maxGPU" placeholder="輸入建議配備顯示卡" class="ms-3 mb-1" /><br /></li>
                                <li><strong>DirectX:</strong><input type="text" v-model="maxDirectX" placeholder="輸入建議配備DirectX" class="ms-3 mb-1" /><br /></li>
                                <li><strong>儲存空間:</strong><input type="text" v-model="maxROM" placeholder="輸入建議配備儲存空間" class="ms-3 mb-1" /><br /></li>
                                <li><strong>音效卡:</strong><input type="text" v-model="maxSoundCard" placeholder="輸入建議配備音效卡" class="ms-3 mb-1" /><br /></li>
                                <li><strong>備註:</strong><input type="text" v-model="maxRemark" placeholder="輸入建議配備備註" class="ms-3 mb-1" /></li>
                            </ul>
                        </ul>
                    </div>
                    <div style="clear: both"></div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
export default {
    name: 'Custom',
    data() {
        return {
            // 最低配備
            minSystem: '',
            minCPU: '',
            minRAM: '',
            minGPU: '',
            minDirectX: '',
            minROM: '',
            minSoundCard: '',
            minRemark: '',

            // 建議配備
            maxSystem: '',
            maxCPU: '',
            maxRAM: '',
            maxGPU: '',
            maxDirectX: '',
            maxROM: '',
            maxSoundCard: '',
            maxRemark: ''
        };
    },
    methods: {
        
    }
};
</script>
