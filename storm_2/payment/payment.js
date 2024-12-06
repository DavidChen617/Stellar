const { createApp } = Vue;

const app = createApp({
  data: function () {
    return {
      pay_status: 1,
      step:[
        {
            step:1,
            here:true,
            "bg-primary":true
          },
          {
            step:2,
            here:false,
            "bg-primary":false
          },
          {
            step:3,
            here:false,
            "bg-primary":false
          }
        
      ],
      
      second_step:{
        here:false,
        "bg-primary":false
      },
      third_step:{
        here:false,
        "bg-primary":false
      },
    };
  },
  methods: {
    change_status: function () {
      if (this.pay_status < 3) {
        this.pay_status++;
      } else {
        this.pay_status = 1;
      }
    },
  },
  computed: {},
  watch: {
    pay_status: {
      handler(newValue,oldValue) {
        this.step.forEach(element => {
            if(element.step==newValue){
                element.here=true;
                element["bg-primary"]=true;
            }else{
                element.here=false;
                element["bg-primary"]=false;
            }
        });



      },
      deep: true,
      immediate: true,
    },
  },
});
app.mount("#app");
