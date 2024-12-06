import { getAllUser } from "@/api/index";
import { updateUserApi } from "@/api/index";
import { editUserApi } from "@/api/index";



export const getAllUserService = {
    getUser(){
        return new Promise((resolve,rejsect)=>{
            getAllUser()
            .then(data=>{
                
                resolve(data.body);
            })
            .catch(err=>{
                rejsect(err);
            })
        })
    },
      // 修改用户信息
      updateUser(userId, islock) {
        return new Promise((resolve, reject) => {
            updateUserApi(userId, islock) // 调用修改用户 API
                .then(data => {
                    resolve(data.body); // 成功返回修改后的数据
                })
                .catch(err => {
                    reject(err); // 处理错误
                });
        });
    },

    // 新增用户
    editUser(userid,nickName,email,country) {
        return new Promise((resolve, reject) => {
            editUserApi(userid,nickName,email,country)  // 调用新增用户 API
                .then(data => {
                    resolve(data.body); // 成功返回新增的用户数据
                })
                .catch(err => {
                    reject(err); // 处理错误
                });
        });
    }

};