import { getAllShippers , updateShipper } from "@/api/index";

export const ShipperService = {
    getShippers(){
        // return fetch('/demo/data/shippers.json').then(res=>res.json())
        
        return new Promise((resolve,reject)=>{
            getAllShippers()
            .then(data=>{
                resolve(data.body);
            })
            .catch(err=>{
                reject(err);
            })
        })
    }
};

