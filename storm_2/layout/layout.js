
var { createApp } = Vue;
const app = createApp({

    data: function () {
        return {
            current_status:false,

        }

    },
    methods: {
        change_status:function(){
            if(this.current_status){
                this.current_status=false;
            }else{
                this.current_status=true;
            }
        }

    },
    computed: { 
    
    }
})

app.mount("#app");

