import Vue from 'vue'
import { Mutation, Action, VuexModule, getModule, Module } from 'vuex-module-decorators'
import { store } from '~/store'
import UserModel from '~/api/models/UserModel'
import { $api } from '~/globals/api'

@Module({
  dynamic: true,
  store,
  name: 'user',
  stateFactory: true,
  namespaced: true
})
class UserModule extends VuexModule {
  // state
  private _user?: UserModel

  // getters
  public get user() {
    return this._user
  }

  // mutations
  @Mutation
  private setUser(user: UserModel) {
    this._user = user
  }

  // actions
  @Action
  public async fetchUser() {
    try {
      const response = await $api.user.getUser()
      if (response.successful) {
        this.setUser(response.data!)
        return response.data!
      }
    } catch (e) {
      Vue.toasted.error('Error while fetching user information')
    }
  }

  @Action
  public async createUser(username: string, password: string, email: string) {
    try {
      const response = await $api.user.createUser(username, password, email)
      if (response.successful) {
        Vue.toasted.success(`Successfully created new user '${ username }'`)
        // TODO
      }
    } catch(e) {
      Vue.toasted.error('Error while creating new user')
    }
  }

  @Action
  public async deleteUser(userId: number) {
    try {
      const response = await $api.user.deleteUser(userId)
      if (response.successful) {
        Vue.toasted.success('User successfully deleted')
        // TODO
      }
    } catch (e) {
      Vue.toasted.error('Error while deleting user')
    }
  }
}

export default getModule(UserModule)
